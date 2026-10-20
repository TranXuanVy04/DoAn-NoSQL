"""Tạo 120 sản phẩm và 72 đơn deterministic; giữ BSON ObjectId/Int64/Date."""
import argparse
from copy import deepcopy
from datetime import datetime, timedelta, timezone
from bson import ObjectId, Int64
from common import ROOT, write_json, expected

def oid(n): return ObjectId(f'{n:024x}')

def dataset():
    categories=[{'_id':oid(1),'name':'Điện tử','kind':'electronics'}, {'_id':oid(2),'name':'Thời trang','kind':'fashion'},{'_id':oid(3),'name':'Gia dụng','kind':'home'}]
    products=[]
    names=[['Tai nghe','Loa Bluetooth','Bàn phím','Chuột không dây','Đèn màn hình'],['Áo thun','Quần dài','Áo khoác','Váy','Quần ngắn'],['Bình giữ nhiệt','Hộp đựng thực phẩm','Nồi inox','Kệ bếp','Bình nước']]
    for c,cat in enumerate(categories):
        for i in range(40):
            attrs=({'brand':['EC Tech','Nova','Soundly'][i%3],'warrantyMonths':12+i%3*6,'connectivity':'Bluetooth' if i%2 else 'USB-C'} if c==0 else
                   {'size':['S','M','L','XL'][i%4],'material':['Cotton','Denim','Linen'][i%3],'color':['Đen','Trắng','Xanh'][i%3]} if c==1 else
                   {'material':['Inox','Nhựa PP','Thủy tinh'][i%3],'origin':'Việt Nam','capacityLiters':1+i%5})
            products.append({'_id':oid(100+c*40+i),'sku':f'{cat["kind"].upper()}-{i+1:03}', 'name':f'{names[c][i%5]} EC {i+1:02}', 'categoryId':cat['_id'], 'kind':cat['kind'],
                             'price':Int64((c+1)*100000+(i+1)*15000),'stock':100+i%30,'isActive':True,'attributes':attrs})
    orders=[]; base=datetime(2026,8,1,tzinfo=timezone.utc)
    for i in range(72):
        # 2–5 dòng khác nhau; một số sản phẩm lặp nhiều lần để có Top 5.
        indices=[i%8]
        for j in range(1,2+i%4):
            idx=(i*7+j*19)%120
            while idx in indices: idx=(idx+1)%120
            indices.append(idx)
        assert len(set(indices))==len(indices)
        items=[]
        for j,idx in enumerate(indices):
            p=products[idx];qty=1+(i+j)%3
            # Một số giá lịch sử khác giá catalog hiện tại để chứng minh snapshot.
            price=Int64(p['price']-(10000 if i%4==0 else 0))
            items.append({'productId':p['_id'],'sku':p['sku'],'name':p['name'],'categoryName':categories[idx//40]['name'],'kind':p['kind'],
                          'attributes':deepcopy(p['attributes']),'unitPrice':price,'quantity':qty,'lineTotal':Int64(price*qty)})
        status='Cancelled' if i%9==0 else 'Pending' if i%5==0 else 'Completed'
        orders.append({'_id':oid(1000+i),'number':f'SEED-2026-{i+1:04}','checkoutKey':f'seed-v1-{i+1:04}','customerToken':f'seed-customer-{i%12}',
                       'customer':{'name':f'Khách hàng mẫu {i%12+1:02}','phone':'0900000000','address':f'{i+1} Đường Mẫu, TP. Hồ Chí Minh'},
                       'createdAt':base+timedelta(days=i,hours=(i*5)%24,minutes=15), 'status':status,'items':items,'total':Int64(sum(x['lineTotal'] for x in items))})
    return {'danhmuc':categories,'sanpham':products,'hoadon':orders}

def validate(data):
    assert len(data['sanpham'])>=100 and len(data['hoadon'])>=50
    assert len({x['kind'] for x in data['sanpham']})>=3
    ids={x['_id'] for x in data['sanpham']}
    for p in data['sanpham']:
        assert isinstance(p['_id'],ObjectId) and isinstance(p['price'],Int64) and p['stock']>=0
    for o in data['hoadon']:
        assert 2<=len(o['items'])<=5
        assert isinstance(o['total'],Int64) and isinstance(o['createdAt'],datetime)
        assert o['total']==sum(x['unitPrice']*x['quantity'] for x in o['items'])
        assert all(x['productId'] in ids and isinstance(x['unitPrice'],Int64) and x['quantity']>0 and x['lineTotal']==x['unitPrice']*x['quantity'] for x in o['items'])

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--out',default=str(ROOT/'data'));args=parser.parse_args()
    from pathlib import Path
    data=dataset();validate(data)
    for name,rows in data.items(): write_json(Path(args.out)/f'{name}.json',rows)
    write_json(Path(args.out)/'expected_report.json',expected(data['hoadon'],'2026-08-01','2026-10-31'))
    print('PASS: 3 danh mục, 120 sản phẩm, 72 đơn; mỗi đơn 2–5 dòng. Đã xuất Extended JSON canonical.')
