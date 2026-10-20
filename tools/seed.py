import argparse
from common import client, DB_NAME, dev_only, COLLECTIONS, ROOT, read_json, canonical, indexes
from generate_data import dataset, validate

def seed(db_name):
    dev_only(db_name);c=client();db=c[db_name];data=dataset();validate(data)
    existing={name:list(db[name].find().sort('_id',1)) for name in COLLECTIONS}
    if any(existing.values()):
        if all(canonical(existing[name])==canonical(sorted(data[name],key=lambda x:x['_id'])) for name in COLLECTIONS):
            print('NO-OP: bộ seed v1 đã có nguyên vẹn; không ghi đè.');return
        raise ValueError('DB đã có dữ liệu khác seed. Không ghi đè. Hãy chọn DB dev/test trống mới.')
    validators=read_json(ROOT/'tools'/'validators.json')
    for name in COLLECTIONS:
        if name not in db.list_collection_names(): db.create_collection(name,validator=validators[name])
        else: db.command({'collMod':name,'validator':validators[name]})
    indexes(db)
    with c.start_session() as s:
        with s.start_transaction():
            for name in COLLECTIONS: db[name].insert_many(data[name],session=s)
    for name in COLLECTIONS: print(f'{name}: {db[name].count_documents({})}')
    print('PASS: seed transaction hoàn tất, validators và indexes đã thiết lập.')

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--db',default=DB_NAME);a=p.parse_args();seed(a.db)
