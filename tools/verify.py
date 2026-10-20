"""Một lệnh chạy kiểm chứng; tạo DB thử riêng, không xóa dữ liệu đã có."""
import argparse
import os
import secrets
import socket
import subprocess
import sys
import time
from datetime import datetime
from pathlib import Path
import requests
from common import ROOT, URI

def main():
    p=argparse.ArgumentParser();p.add_argument('--port',type=int,default=5081);p.add_argument('--scale',type=int,default=50);a=p.parse_args()
    stamp=datetime.now().strftime('%Y%m%d_%H%M%S')+'_'+secrets.token_hex(2)
    test=f'sv3_{stamp}_test';source=f'sv3_{stamp}_dev';target=f'sv3_{stamp}_bench';restore=f'sv3_{stamp}_restore_test'
    with socket.socket() as sock:
        try: sock.bind(('127.0.0.1',a.port))
        except OSError: raise RuntimeError(f'Cổng {a.port} đang bận. Dừng web test cũ hoặc dùng --port khác.')
    env=dict(os.environ,MONGO_DB=test,Mongo__Database=test,Mongo__ConnectionString=URI,ASPNETCORE_ENVIRONMENT='Development',Admin__Username='admin')
    password=secrets.token_urlsafe(24);env['Admin__Password']=password;env['EC_ADMIN_PASSWORD']=password
    env['EC_BASE_URL']=f'http://localhost:{a.port}'
    def run(*args):subprocess.run(args,cwd=ROOT,env=env,check=True)
    run(sys.executable,'tools/init_mongo.py')
    run(sys.executable,'tools/generate_data.py')
    run(sys.executable,'-m','unittest','discover','-s','tests','-p','test_*.py','-v')
    run('dotnet','build','src/Ecommerce.Web','-c','Release','--nologo')
    run(sys.executable,'tools/seed.py','--db',test)
    run(sys.executable,'tools/seed.py','--db',test)
    (ROOT/'evidence').mkdir(exist_ok=True)
    with (ROOT/'evidence'/'web-verify.log').open('w',encoding='utf-8') as log:
        process=subprocess.Popen(['dotnet','run','--project','src/Ecommerce.Web','-c','Release','--no-build','--urls',env['EC_BASE_URL']],cwd=ROOT,env=env,stdout=log,stderr=subprocess.STDOUT)
        try:
            for _ in range(60):
                if process.poll() is not None:raise RuntimeError('Web dừng khởi động; xem evidence/web-verify.log')
                try:
                    if requests.get(env['EC_BASE_URL'],timeout=2).status_code==200:break
                except requests.RequestException:pass
                time.sleep(.5)
            else:raise TimeoutError('Web chưa sẵn sàng')
            run(sys.executable,'tests/e2e.py')
        finally:
            process.terminate()
            try:process.wait(timeout=10)
            except subprocess.TimeoutExpired:process.kill();process.wait()
    run(sys.executable,'tools/seed.py','--db',source)
    run(sys.executable,'tools/transfer_data.py','roundtrip','--db',source,'--target',restore,'--folder',str(ROOT/'exports'/stamp))
    run(sys.executable,'tools/benchmark.py','--source',source,'--target',target,'--scale',str(a.scale),'--out',str(ROOT/'evidence'/'benchmark.json'))
    print('ALL CHECKS PASSED. DB thử được giữ để kiểm tra lại:',test,source,target,restore)
    print('Không có DB/index nguồn dùng chung nào bị xóa. Xem evidence/e2e.json và evidence/benchmark.json.')

if __name__=='__main__':main()
