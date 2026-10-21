"use client";
import { useEffect, useState } from 'react';
import type { ProductPage } from '../../shared/api/client';
import { getProducts } from './service';
export default function ProductList() {
  const [data, setData] = useState<ProductPage>();
  const [error, setError] = useState('');
  useEffect(() => { let active = true; getProducts().then(value => {if(active) setData(value);}).catch(() => {if(active) setError('Không tải được sản phẩm. Kiểm tra API và thử lại.');}); return () => {active=false;}; }, []);
  if(error) return <p role="alert">{error}</p>;
  if(!data) return <p>Đang tải sản phẩm…</p>;
  if(!data.items.length) return <p>Chưa có sản phẩm.</p>;
  return <div className="grid">{data.items.map(p => <article key={p.id}><small>{p.sku}</small><h2>{p.name}</h2><p>{new Intl.NumberFormat('vi-VN',{style:'currency',currency:'VND'}).format(p.price)}</p><small>Còn {p.stock} sản phẩm</small></article>)}</div>;
}
