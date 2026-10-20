"""Khởi tạo replica set local một node để demo transaction, chờ writable primary."""
import time
from pymongo import MongoClient
from pymongo.errors import OperationFailure, PyMongoError

def main():
    c=MongoClient('mongodb://localhost:27017/?directConnection=true',serverSelectionTimeoutMS=1000)
    deadline=time.monotonic()+60
    while True:
        try:
            c.admin.command('ping');break
        except PyMongoError:
            if time.monotonic()>deadline:raise
            time.sleep(.5)
    try:c.admin.command('replSetInitiate',{'_id':'rs0','members':[{'_id':0,'host':'localhost:27017'}]})
    except OperationFailure as ex:
        if ex.code!=23:raise
    while time.monotonic()<deadline:
        if c.admin.command('hello').get('isWritablePrimary'):
            print('PASS: rs0 writable primary sẵn sàng.');return
        time.sleep(.5)
    raise TimeoutError('Replica set chưa bầu được primary sau 60 giây.')

if __name__=='__main__':main()
