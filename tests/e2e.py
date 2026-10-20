"""Chạy với web + MongoDB thật trên DB _test riêng. Không dùng DB nhóm/demo."""
import os
import sys
import uuid
from pathlib import Path
from datetime import datetime, timezone
from concurrent.futures import ThreadPoolExecutor
import requests
from bs4 import BeautifulSoup
sys.path.insert(0,str(Path(__file__).resolve().parents[1]/'tools'))
from common import client, DB_NAME, canonical, expected, pipelines, write_json, ROOT

URL=os.environ.get('EC_BASE_URL','http://localhost:5080').rstrip('/')
assert DB_NAME.endswith('_test'),'E2E chỉ chạy DB _test; web phải dùng cùng DB.'
PASSWORD=os.environ['EC_ADMIN_PASSWORD'];db=client()[DB_NAME];checks=[]

def check(name, condition):
    assert condition,name
    checks.append(name);print('PASS:',name)

def get(session,path):
    r=session.get(URL+path,timeout=30);assert r.status_code==200,(path,r.status_code,r.text[:200]);return r

def token(r):
    t=BeautifulSoup(r.text,'html.parser').select_one('input[name="__RequestVerificationToken"]');assert t,'Missing CSRF token';return t['value']

def post(session,path,data,form=None):
    r=form or get(session,path)
    return session.post(URL+path,data=dict(data,__RequestVerificationToken=token(r)),timeout=30)

def login():
    s=requests.Session();r=post(s,'/Account/Login',{'Username':'admin','Password':PASSWORD})
    check('Đăng nhập Admin',r.status_code==200 and '/Admin/Dashboard' in r.url)
    return s

def create_product(admin,cat,sku,stock=20):
    f={'Sku':sku,'Name':'Sản phẩm E2E '+sku,'CategoryId':str(cat['_id']),'Price':'150000','Stock':str(stock),'IsActive':'true','AttributesJson':'{"brand":"E2E","warrantyMonths":12}'}
    r=post(admin,'/Admin/ProductEdit',f);assert r.status_code==200 and '/Admin/Products' in r.url,r.text[:300]
    return db.sanpham.find_one({'sku':sku}),f

def checkout(session,products):
    for p in products:
        r=post(session,'/Cart/Add',{'productId':str(p['_id']),'quantity':'1'},get(session,'/Catalog/Details/'+str(p['_id'])))
        assert r.status_code==200
    page=get(session,'/Cart/Checkout');key=BeautifulSoup(page.text,'html.parser').select_one('input[name="CheckoutKey"]')['value']
    return page,{'CheckoutKey':key,'Name':'Khách kiểm thử','Phone':'0900000000','Address':'123 Đường Kiểm Thử, TP Hồ Chí Minh'}

def main():
    guest=requests.Session();admin=login();mark=uuid.uuid4().hex[:8]
    r=guest.get(URL+'/Admin/Products',allow_redirects=False);check('Admin yêu cầu xác thực',r.status_code==302 and '/Account/Login' in r.headers['Location'])
    r=guest.post(URL+'/Cart/Add',data={'productId':'invalid','quantity':1});check('POST thiếu anti-forgery bị chặn',r.status_code==400)
    category_name='Danh mục kiểm thử '+mark
    post(admin,'/Admin/CategoryEdit',{'Name':category_name,'Kind':'home'})
    tempcat=db.danhmuc.find_one({'name':category_name})
    check('Tạo danh mục qua MVC',tempcat is not None)
    duplicate=post(admin,'/Admin/CategoryEdit',{'Name':category_name,'Kind':'home'})
    check('Chặn danh mục trùng tên','validation-summary-errors' in duplicate.text)
    post(admin,'/Admin/CategoryEdit',{'Id':str(tempcat['_id']),'Name':category_name+' sửa','Kind':'home'})
    check('Sửa danh mục qua MVC',db.danhmuc.find_one({'_id':tempcat['_id']})['name']==category_name+' sửa')
    post(admin,'/Admin/CategoryDelete',{'id':str(tempcat['_id'])},get(admin,'/Admin/Categories'))
    check('Xóa danh mục rỗng qua MVC',db.danhmuc.find_one({'_id':tempcat['_id']}) is None)
    cat=db.danhmuc.find_one({'kind':'electronics'})
    get(guest,'/Catalog/Index?page=2');check('Phân trang catalog hoạt động',True)
    r=get(guest,'/Catalog/Index?q='+mark);check('Catalog rỗng hiển thị rõ', 'Chưa có sản phẩm phù hợp' in r.text)
    p1,f1=create_product(admin,cat,'TEST-'+mark+'-1');p2,f2=create_product(admin,cat,'TEST-'+mark+'-2')
    check('Tạo sản phẩm từ form MVC ghi DB thật',p1 is not None and p2 is not None)
    bad=dict(f1,Sku='BAD-'+mark,AttributesJson='{"size":"M"}')
    r=post(admin,'/Admin/ProductEdit',bad);check('Validation thuộc tính theo ngành',r.status_code==200 and db.sanpham.find_one({'sku':bad['Sku']}) is None and 'validation-summary-errors' in r.text)
    malformed=post(admin,'/Admin/ProductEdit',dict(bad,AttributesJson='{not-json'))
    check('JSON sai định dạng trả lỗi form',malformed.status_code==200 and 'validation-summary-errors' in malformed.text)
    r=post(admin,'/Admin/ProductEdit',f1);check('SKU trùng bị chặn', 'validation-summary-errors' in r.text)
    r=post(admin,'/Admin/CategoryDelete',{'id':str(cat['_id'])},get(admin,'/Admin/Categories'))
    check('Không xóa danh mục còn sản phẩm',db.danhmuc.find_one({'_id':cat['_id']}) is not None and 'Danh mục còn sản phẩm' in BeautifulSoup(r.text,'html.parser').get_text())
    page,data=checkout(guest,[p1,p2]);r=post(guest,'/Cart/Checkout',data,page)
    order=db.hoadon.find_one({'checkoutKey':data['CheckoutKey']})
    check('Checkout nhúng giá/thuộc tính và trừ kho',order is not None and order['total']==300000 and len(order['items'])==2 and db.sanpham.find_one({'_id':p1['_id']})['stock']==19)
    r=post(guest,'/Cart/Checkout',data,page)
    check('Bấm lại checkout không tạo trùng',db.hoadon.count_documents({'checkoutKey':data['CheckoutKey']})==1 and db.sanpham.find_one({'_id':p1['_id']})['stock']==19 and r.status_code==200)
    other=requests.Session();r=other.get(URL+'/Orders/Details/'+str(order['_id']))
    check('Khách khác không xem được đơn',r.status_code==404)
    snapshot=canonical(order['items']);edit=dict(f1,Id=str(p1['_id']),Name='Tên đã thay đổi',Price='250000',Stock='19',OriginalStock='19',AttributesJson='{"brand":"Sau khi mua","warrantyMonths":24}')
    post(admin,'/Admin/ProductEdit',dict(edit,OriginalStock='20'),get(admin,'/Admin/ProductEdit/'+str(p1['_id'])))
    check('Form cũ không ghi đè tồn kho vừa mua',db.sanpham.find_one({'_id':p1['_id']})['price']==150000 and db.sanpham.find_one({'_id':p1['_id']})['stock']==19)
    r=post(admin,'/Admin/ProductEdit',edit,get(admin,'/Admin/ProductEdit/'+str(p1['_id'])))
    check('Sửa giá và thuộc tính sản phẩm thành công',db.sanpham.find_one({'_id':p1['_id']})['price']==250000)
    check('Đơn cũ giữ nguyên snapshot',canonical(db.hoadon.find_one({'_id':order['_id']})['items'])==snapshot)
    detail=get(admin,'/Orders/Details/'+str(order['_id']))
    r=post(admin,'/Admin/OrderStatus',{'id':str(order['_id']),'status':'Completed'},detail)
    check('Hoàn tất đơn',db.hoadon.find_one({'_id':order['_id']})['status']=='Completed')
    # Đối chiếu Python độc lập, Mongo pipeline và HTML dashboard cùng khoảng.
    start,end='2026-01-01','2026-12-31';all_orders=list(db.hoadon.find());exp=expected(all_orders,start,end)
    for group in ('day','month'):
        e=expected(all_orders,start,end,group)
        for name,pipeline in pipelines(start,end,group).items():
            actual=list(db.hoadon.aggregate(pipeline));check(f'Aggregation {name}/{group} khớp tính độc lập',actual==e[name])
    r=get(admin,f'/Admin/Dashboard?from={start}&to={end}');soup=BeautifulSoup(r.text,'html.parser')
    check('Dashboard doanh thu khớp DB',int(soup.select_one('#revenue-total')['data-value'])==sum(x['revenue'] for x in exp['revenue']))
    check('Dashboard số đơn khớp DB',int(soup.select_one('#order-count')['data-value'])==sum(x['orders'] for x in exp['revenue']))
    check('Dashboard Top 5 khớp DB',[x['data-product'] for x in soup.select('#top-products tr[data-product]')]==[str(x['_id']) for x in exp['top5']])
    r=get(admin,'/Admin/Dashboard?from=2025-01-01&to=2025-01-01');check('Dashboard khoảng rỗng',BeautifulSoup(r.text,'html.parser').select_one('#revenue-total')['data-value']=='0')
    r=admin.get(URL+'/Admin/Dashboard?from=2026-10-01&to=2026-01-01');check('Dashboard chặn khoảng ngày đảo',r.status_code==400)
    # Hủy lặp không hoàn kho hai lần.
    before=db.sanpham.find_one({'_id':p2['_id']})['stock'];page,d=checkout(guest,[p2]);post(guest,'/Cart/Checkout',d,page);o=db.hoadon.find_one({'checkoutKey':d['CheckoutKey']})
    detail=get(admin,'/Orders/Details/'+str(o['_id']))
    for _ in range(2):post(admin,'/Admin/OrderStatus',{'id':str(o['_id']),'status':'Cancelled'},detail)
    check('Hủy đơn hoàn kho đúng một lần',db.sanpham.find_one({'_id':p2['_id']})['stock']==before)
    # Hai khách cạnh tranh sản phẩm cuối; cả hai đã có trong giỏ trước khi đặt.
    last,_=create_product(admin,cat,'LAST-'+mark,stock=1)
    a,b=requests.Session(),requests.Session();fa,da=checkout(a,[last]);fb,dbb=checkout(b,[last]);keys=[da['CheckoutKey'],dbb['CheckoutKey']]
    with ThreadPoolExecutor(max_workers=2) as pool:
        results=list(pool.map(lambda args:post(*args),[(a,'/Cart/Checkout',da,fa),(b,'/Cart/Checkout',dbb,fb)]))
    check('Hai checkout đồng thời không bán vượt tồn',db.hoadon.count_documents({'checkoutKey':{'$in':keys}})==1 and db.sanpham.find_one({'_id':last['_id']})['stock']==0 and all(x.status_code==200 for x in results))
    pa,_=create_product(admin,cat,'ROLLBACK-'+mark+'-1',stock=5)
    pb,_=create_product(admin,cat,'ROLLBACK-'+mark+'-2',stock=5)
    rollback_guest=requests.Session();form,body=checkout(rollback_guest,[pa,pb])
    db.sanpham.update_one({'_id':pb['_id']},{'$set':{'stock':0}})
    response=post(rollback_guest,'/Cart/Checkout',body,form)
    check('Hết kho một dòng rollback toàn bộ đơn và kho',response.status_code==200 and db.hoadon.count_documents({'checkoutKey':body['CheckoutKey']})==0 and db.sanpham.find_one({'_id':pa['_id']})['stock']==5)
    r=post(admin,'/Admin/ProductDelete',{'id':str(p2['_id'])},get(admin,'/Admin/Products'))
    check('Ngừng bán ẩn khỏi catalog, giữ đơn cũ',not db.sanpham.find_one({'_id':p2['_id']})['isActive'] and guest.get(URL+'/Catalog/Details/'+str(p2['_id'])).status_code==404 and db.hoadon.find_one({'_id':order['_id']}) is not None)
    write_json(ROOT/'evidence'/'e2e.json',{'at':datetime.now(timezone.utc),'database':DB_NAME,'baseUrl':URL,'passed':len(checks),'checks':checks})
    print(f'ALL PASS: {len(checks)} kiểm tra với HTTP + MongoDB thật.')

if __name__=='__main__':main()
