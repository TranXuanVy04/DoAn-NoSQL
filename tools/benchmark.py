"""Đo đọc hết cursor trước/sau index trên bản sao, giữ nguyên indexes DB nguồn."""
import argparse
import math
import platform
import statistics
import time
from datetime import datetime, timezone
from bson import ObjectId
from common import client, dev_only, pipelines, canonical, write_json, ROOT

def percentile95(samples): return sorted(samples)[math.ceil(.95*len(samples))-1]

def run(source,target,start,end,scale,warmup,runs,out):
    dev_only(target)
    if source==target or not target.endswith('_bench'): raise ValueError('DB benchmark phải khác nguồn và kết thúc _bench.')
    c=client();db=c[target]
    if db.list_collection_names(): raise ValueError('Chọn DB _bench mới/trống; không drop index của DB dùng chung.')
    src=list(c[source].hoadon.find());assert src,'DB nguồn chưa có đơn'
    source_fingerprint=canonical(src)
    docs=[]
    for batch in range(scale):
        for row in src:
            copy=dict(row);copy['_id']=ObjectId();copy['number']=f'BENCH-{batch}-{row["number"]}';copy['checkoutKey']=f'bench-{batch}-{row["checkoutKey"]}';docs.append(copy)
    db.hoadon.insert_many(docs)
    queries=pipelines(start,end)
    result={'measuredAt':datetime.now(timezone.utc),'serverVersion':c.server_info()['version'],'platform':platform.platform(),'python':platform.python_version(),'source':source,'target':target,'documents':len(docs),'scale':scale,'warmup':warmup,'runs':runs,'from':start,'toInclusive':end,'timezone':'+07:00','method':'perf_counter_ns; materialize full cursor; nearest-rank p95; before then after on isolated copy','phases':{}}
    baseline={}
    for phase in ('before','after'):
        if phase=='after': db.hoadon.create_index([('status',1),('createdAt',1)],name='ix_order_status_date')
        phase_result={}
        for name,pipeline in queries.items():
            for _ in range(warmup): list(db.hoadon.aggregate(pipeline))
            samples=[];last=None
            for _ in range(runs):
                t=time.perf_counter_ns();last=list(db.hoadon.aggregate(pipeline));samples.append((time.perf_counter_ns()-t)/1e6)
            if phase=='before': baseline[name]=canonical(last)
            else: assert baseline[name]==canonical(last),name+' result changed after index'
            explain=db.command('explain',{'aggregate':'hoadon','pipeline':pipeline,'cursor':{}},verbosity='executionStats')
            phase_result[name]={'medianMs':statistics.median(samples),'p95Ms':percentile95(samples),'minMs':min(samples),'samplesMs':samples,'resultRows':len(last),'explain':explain,'resultUnchanged':phase=='after'}
            print(f'{phase:6} {name:7} median={statistics.median(samples):.3f}ms p95={percentile95(samples):.3f}ms')
        result['phases'][phase]=phase_result
    assert source_fingerprint==canonical(list(c[source].hoadon.find())),'Nguồn thay đổi trong lúc đo; chạy lại khi không có ghi đồng thời.'
    result['sourceUnchanged']=True
    write_json(out,result)
    print('PASS: kết quả trước/sau giống nhau; DB nguồn không đổi. Report:',out)

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--source',default='ecommerce_dev');p.add_argument('--target',required=True);p.add_argument('--from',dest='start',default='2026-08-01');p.add_argument('--to',dest='end',default='2026-10-31');p.add_argument('--scale',type=int,default=1);p.add_argument('--warmup',type=int,default=5);p.add_argument('--runs',type=int,default=30);p.add_argument('--out',default=str(ROOT/'evidence'/'benchmark.json'));a=p.parse_args()
    if a.scale<1 or a.warmup<1 or a.runs<10:p.error('scale/warmup >=1, runs >=10')
    run(a.source,a.target,a.start,a.end,a.scale,a.warmup,a.runs,a.out)
