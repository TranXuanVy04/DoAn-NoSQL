"""Export/import canonical EJSON vào DB mới; không drop/replace dữ liệu cũ."""
import argparse
from pathlib import Path
from common import client, COLLECTIONS, dev_only, read_json, write_json, canonical, indexes

def export(db, folder):
    folder=Path(folder);folder.mkdir(parents=True,exist_ok=False)
    for name in COLLECTIONS: write_json(folder/f'{name}.json',list(db[name].find().sort('_id',1)))
    print('Export:',folder)

def restore(db, folder):
    dev_only(db.name)
    if any(db[name].count_documents({}) for name in COLLECTIONS): raise ValueError('DB đích phải trống. Không ghi đè dữ liệu.')
    data={name:read_json(Path(folder)/f'{name}.json') for name in COLLECTIONS}
    from common import ROOT
    validators=read_json(ROOT/'tools'/'validators.json')
    for name in COLLECTIONS:
        if name not in db.list_collection_names(): db.create_collection(name,validator=validators[name])
        else: db.command({'collMod':name,'validator':validators[name]})
    indexes(db)
    with db.client.start_session() as session:
        with session.start_transaction():
            for name in COLLECTIONS:
                if data[name]: db[name].insert_many(data[name],session=session)
    for name in COLLECTIONS:
        actual=list(db[name].find().sort('_id',1))
        assert canonical(actual)==canonical(sorted(data[name],key=lambda x:x['_id'])),name+' BSON mismatch'
        print(f'PASS {name}: {len(actual)} documents; BSON/content exact')

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('action',choices=['export','import','roundtrip']);p.add_argument('--db',default='ecommerce_dev');p.add_argument('--folder',required=True);p.add_argument('--target');a=p.parse_args();c=client()
    if a.action in ('export','roundtrip'): export(c[a.db],a.folder)
    if a.action=='import': restore(c[a.db],a.folder)
    if a.action=='roundtrip':
        if not a.target or a.target==a.db: p.error('--target phải là DB test mới, khác DB nguồn')
        restore(c[a.target],a.folder)
        for name in COLLECTIONS: assert canonical(list(c[a.db][name].find().sort('_id',1)))==canonical(list(c[a.target][name].find().sort('_id',1)))
        print('PASS roundtrip: count, ObjectId, Date, Int64 và toàn bộ nội dung không đổi.')
