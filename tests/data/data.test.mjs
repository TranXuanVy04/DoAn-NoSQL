import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
function revive(_key, value) {
  if (!value || typeof value !== 'object') return value;
  if ('$oid' in value) return value.$oid;
  if ('$numberLong' in value) return BigInt(value.$numberLong);
  if ('$numberInt' in value) return Number(value.$numberInt);
  if ('$date' in value) return new Date(typeof value.$date === 'bigint' ? Number(value.$date) : value.$date);
  return value;
}
async function load(name) { return JSON.parse(await readFile(new URL('../../data/'+name+'.json',import.meta.url),'utf8'),revive); }
const [categories,products,orders,expected] = await Promise.all(['danhmuc','sanpham','hoadon','expected_report'].map(load));
test('Dataset meets quota and has unique identifiers', () => {
  assert.ok(products.length >= 100);
  assert.ok(orders.length >= 50 && orders.length <= 100);
  assert.ok(new Set(products.map(p=>p.categoryId)).size >= 3);
  for (const rows of [categories,products,orders]) assert.equal(new Set(rows.map(x=>x._id)).size,rows.length);
  const skus = products.flatMap(p => p.variants.length ? p.variants.map(v => v.sku) : [p.sku]);
  assert.equal(new Set(skus).size, skus.length);
  assert.equal(new Set(orders.map(o=>o.checkoutKey)).size,orders.length);
});
test('Seed has valid BSON money, stock and product references', () => {
  const ids = new Set(products.map(p=>p._id));
  const categoryIds = new Set(categories.map(c=>c._id));
  for (const p of products) {
    assert.ok(Array.isArray(p.variants));
    assert.ok(categoryIds.has(p.categoryId));
    if (p.variants.length) {
      assert.ok(typeof p.sku === 'string' && p.sku.length > 0);
      for (const field of ['price', 'stock']) assert.ok(!Object.hasOwn(p, field));
      assert.equal(new Set(p.variants.map(v => v.id)).size, p.variants.length);
    }
    for (const row of p.variants.length ? p.variants : [p]) {
      assert.equal(typeof row.price, 'bigint'); assert.ok(row.price > 0n);
      assert.ok(Number.isInteger(row.stock) && row.stock >= 0);
    }
  }
  for (const o of orders) {
    assert.ok(o.items.length >= 2 && o.items.length <= 5);
    assert.ok(o.createdAt instanceof Date && !Number.isNaN(o.createdAt.getTime()));
    assert.ok(['Pending','Completed','Cancelled'].includes(o.status));
    for (const item of o.items) {
      assert.ok(ids.has(item.productId)); assert.ok(Number.isInteger(item.quantity) && item.quantity > 0);
      assert.equal(item.lineTotal,item.unitPrice * BigInt(item.quantity));
    }
    assert.equal(o.total,o.items.reduce((total,item)=>total+item.lineTotal,0n));
  }
});
test('Changing catalog does not change embedded order snapshots', () => {
  const copy = structuredClone({products,orders});
  const before = structuredClone(copy.orders[0].items[0]);
  const p = copy.products.find(p=>p._id === before.productId);
  p.price = 999999n; p.name = 'Changed'; p.attributes.brand = 'Changed';
  assert.deepEqual(copy.orders[0].items[0],before);
});
function report() {
  const revenue = new Map(); const top = new Map();
  const start = Date.parse('2026-07-31T17:00:00Z'); const end = Date.parse('2026-10-31T17:00:00Z');
  const selected = orders.filter(o=>o.status === 'Completed' && o.createdAt.getTime() >= start && o.createdAt.getTime() < end)
    .toSorted((a,b)=>b.createdAt-a.createdAt || b._id.localeCompare(a._id));
  for(const o of selected) {
    const key = new Date(o.createdAt.getTime()+7*3600000).toISOString().slice(0,10);
    const point = revenue.get(key) || {_id:key,revenue:0n,orders:0};
    point.revenue += o.total; point.orders++; revenue.set(key,point);
    for(const item of o.items) {
      const row = top.get(item.productId) || {_id:item.productId,name:item.name,quantity:0,revenue:0n};
      row.quantity += item.quantity; row.revenue += item.unitPrice*BigInt(item.quantity); top.set(item.productId,row);
    }
  }
  return {revenue:[...revenue.values()].sort((a,b)=>a._id.localeCompare(b._id)),top5:[...top.values()].sort((a,b)=>b.quantity-a.quantity || (a.revenue === b.revenue ? a._id.localeCompare(b._id) : a.revenue > b.revenue ? -1 : 1)).slice(0,5)};
}
test('Completed revenue matches expected report in UTC+7', () => {
  const actual = report();
  assert.deepEqual(actual.revenue,expected.revenue.map(x=>({...x,revenue:BigInt(x.revenue)})));
});
test('Snapshot top 5 and deterministic tie breaks match expected report', () => {
  assert.deepEqual(report().top5,expected.top5.map(x=>({...x,revenue:BigInt(x.revenue)})));
});

test('Category parents are valid and never cyclic', () => {
  const byId = new Map(categories.map(c=>[c._id,c]));
  for(const category of categories) {
    assert.ok(!Object.hasOwn(category,'kind'));
    const visited = new Set([category._id]);
    let parent = category.parentId;
    while(parent != null) {
      assert.ok(byId.has(parent)); assert.ok(!visited.has(parent)); visited.add(parent);
      parent = byId.get(parent).parentId;
    }
  }
});
