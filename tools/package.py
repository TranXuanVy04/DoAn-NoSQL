"""Đóng gói source và tùy chọn publish; loại cache, secret và dữ liệu runtime."""
import argparse
import hashlib
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
from common import ROOT

def main():
    p=argparse.ArgumentParser();p.add_argument('--out',required=True);p.add_argument('--with-publish',action='store_true');a=p.parse_args()
    out=Path(a.out).resolve();out.parent.mkdir(parents=True,exist_ok=True)
    skipped={'.git','.venv','bin','obj','__pycache__','exports','artifacts','.vs'}
    if not a.with_publish:skipped.add('publish')
    hashes=[];count=0
    with ZipFile(out,'w',compression=ZIP_DEFLATED,compresslevel=8) as archive:
        for path in sorted(ROOT.rglob('*')):
            rel=path.relative_to(ROOT)
            if not path.is_file() or path.is_symlink() or path.resolve()==out or any(part in skipped for part in rel.parts):continue
            if path.name=='.env' or path.suffix in ('.pyc','.user','.zip') or str(rel)=='publish/Ecommerce.Web':continue
            blob=path.read_bytes();archive.writestr('Ecommerce-SV3/'+rel.as_posix(),blob)
            hashes.append(hashlib.sha256(blob).hexdigest()+'  '+rel.as_posix());count+=1
        archive.writestr('Ecommerce-SV3/CONTENTS.sha256','\n'.join(hashes)+'\n')
    with ZipFile(out) as archive:assert archive.testzip() is None
    print(f'PASS: {out.name}; {count} files + manifest; {out.stat().st_size:,} bytes; ZIP CRC verified.')

if __name__=='__main__':main()
