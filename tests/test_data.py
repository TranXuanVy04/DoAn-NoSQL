import unittest
import sys
from copy import deepcopy
from datetime import datetime, timezone
from pathlib import Path
from bson import Int64
sys.path.insert(0,str(Path(__file__).resolve().parents[1]/'tools'))
from generate_data import dataset, validate
from common import canonical, expected, read_json, pipelines, ROOT, dev_only

class DataTests(unittest.TestCase):
    def test_quota_and_integrity(self): validate(dataset())
    def test_reproducible(self): self.assertEqual(canonical(dataset()),canonical(dataset()))
    def test_embedded_independence(self):
        d=dataset();before=deepcopy(d['hoadon'][0]['items'][0]);p=next(x for x in d['sanpham'] if x['_id']==before['productId'])
        p['name']='Changed';p['price']=Int64(999999);p['attributes']['brand']='Changed'
        self.assertEqual(d['hoadon'][0]['items'][0],before)
    def test_canonical_files(self):
        for name,rows in dataset().items(): self.assertEqual(canonical(read_json(ROOT/'data'/f'{name}.json')),canonical(rows))
    def test_day_boundary_and_status(self):
        orders=[]
        for hour,status in [(16,'Completed'),(17,'Completed'),(18,'Cancelled'),(19,'Pending')]:
            orders.append({'_id':hour,'createdAt':datetime(2026,8,1,hour,tzinfo=timezone.utc),'status':status,'total':100,'items':[]})
        self.assertEqual(expected(orders,'2026-08-01','2026-08-01')['revenue'],[{'_id':'2026-08-01','revenue':100,'orders':1}])
        self.assertEqual(expected(orders,'2026-08-02','2026-08-02')['revenue'],[{'_id':'2026-08-02','revenue':100,'orders':1}])
        self.assertEqual(expected(orders,'2025-01-01','2025-01-01')['revenue'],[])
    def test_pipeline_no_join_and_deterministic_sort(self):
        p=pipelines('2026-08-01','2026-10-31');self.assertNotIn('$lookup',canonical(p));self.assertEqual(p['top5'][-1],{'$limit':5})
        self.assertEqual(p['top5'][-2]['$sort'],{'quantity':-1,'revenue':-1,'_id':1})
    def test_write_guard(self):
        for name in ('production','admin','ecommerce','foo_dev;drop'):
            with self.assertRaises(ValueError):dev_only(name)
        dev_only('ecommerce_test')

if __name__=='__main__':unittest.main(verbosity=2)
