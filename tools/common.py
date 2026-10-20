"""BSON, pipeline và quy tắc ngày dùng chung cho các công cụ kiểm chứng SV3."""
import os
import re
from datetime import datetime, timedelta, timezone
from pathlib import Path
from bson import json_util
from bson.json_util import CANONICAL_JSON_OPTIONS
from pymongo import MongoClient

ROOT = Path(__file__).resolve().parents[1]
URI = os.environ.get('MONGO_URI', 'mongodb://localhost:27017/?replicaSet=rs0&directConnection=true')
DB_NAME = os.environ.get('MONGO_DB', 'ecommerce_dev')
COLLECTIONS = ('danhmuc', 'sanpham', 'hoadon')
VN = timezone(timedelta(hours=7))

def client():
    return MongoClient(URI, serverSelectionTimeoutMS=10000)

def dev_only(name):
    if not re.fullmatch(r'[A-Za-z][A-Za-z0-9_]*_(dev|test|bench)', name):
        raise ValueError('Chỉ ghi DB có tên kết thúc _dev, _test hoặc _bench.')

def canonical(value):
    return json_util.dumps(value, json_options=CANONICAL_JSON_OPTIONS, ensure_ascii=False, indent=2, sort_keys=True)

def write_json(path, value):
    path=Path(path); path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(canonical(value)+'\n', encoding='utf-8')

def read_json(path):
    return json_util.loads(Path(path).read_text(encoding='utf-8'))

def date_range(start, end):
    a=datetime.fromisoformat(start).replace(tzinfo=VN)
    b=datetime.fromisoformat(end).replace(tzinfo=VN)+timedelta(days=1)
    if b<=a: raise ValueError('Ngày kết thúc phải >= ngày bắt đầu')
    return a.astimezone(timezone.utc), b.astimezone(timezone.utc)

def pipelines(start, end, group='day'):
    a,b=date_range(start,end)
    match={'$match': {'status':'Completed','createdAt':{'$gte':a,'$lt':b}}}
    revenue=[match,{'$group':{'_id':{'$dateToString':{'format':'%Y-%m' if group=='month' else '%Y-%m-%d', 'date':'$createdAt','timezone':'+07:00'}},'revenue':{'$sum':'$total'},'orders':{'$sum':1}}},{'$sort':{'_id':1}}]
    top=[match,{'$sort':{'createdAt':-1,'_id':-1}},{'$unwind':'$items'},
         {'$group':{'_id':'$items.productId','name':{'$first':'$items.name'},'quantity':{'$sum':'$items.quantity'},'revenue':{'$sum':{'$multiply':['$items.unitPrice','$items.quantity']}}}},
         {'$sort':{'quantity':-1,'revenue':-1,'_id':1}},{'$limit':5}]
    return {'revenue':revenue,'top5':top}

def expected(orders, start, end, group='day'):
    a,b=date_range(start,end); revenue={}; products={}
    selected=sorted((o for o in orders if o['status']=='Completed' and a<=o['createdAt'].replace(tzinfo=timezone.utc)<b),key=lambda o:(o['createdAt'],o['_id']),reverse=True)
    for o in selected:
        key=o['createdAt'].replace(tzinfo=timezone.utc).astimezone(VN).strftime('%Y-%m' if group=='month' else '%Y-%m-%d')
        r=revenue.setdefault(key,{'_id':key,'revenue':0,'orders':0});r['revenue']+=o['total'];r['orders']+=1
        for item in o['items']:
            p=products.setdefault(item['productId'],{'_id':item['productId'],'name':item['name'],'quantity':0,'revenue':0})
            p['quantity']+=item['quantity'];p['revenue']+=item['unitPrice']*item['quantity']
    return {'revenue':sorted(revenue.values(),key=lambda x:x['_id']),
            'top5':sorted(products.values(),key=lambda x:(-x['quantity'],-x['revenue'],x['_id']))[:5]}

def indexes(db):
    db.danhmuc.create_index('name',unique=True,name='uq_category_name')
    db.sanpham.create_index('sku',unique=True,name='uq_product_sku')
    db.sanpham.create_index([('categoryId',1),('isActive',1),('price',1)],name='ix_catalog_category_active_price')
    db.hoadon.create_index('checkoutKey',unique=True,name='uq_checkout_key')
    db.hoadon.create_index('number',unique=True,name='uq_order_number')
    db.hoadon.create_index([('status',1),('createdAt',1)],name='ix_order_status_date')
