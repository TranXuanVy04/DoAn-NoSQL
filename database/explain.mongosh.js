// Read-only query plan. Run from repo root with MONGO_DB pointing to your test database.
const name = process.env.MONGO_DB || 'ecommerce_dev';
if (!/^[A-Za-z][A-Za-z0-9_]*_(dev|test|bench)$/.test(name)) throw new Error('Use a development/test database.');
const target = db.getSiblingDB(name);
printjson(target.sanpham.find({categoryId:ObjectId('000000000000000000000001'),isActive:true}).sort({price:1}).limit(12).explain('executionStats'));
printjson(target.hoadon.explain('executionStats').aggregate([
 {$match:{status:'Completed',createdAt:{$gte:ISODate('2026-07-31T17:00:00Z'),$lt:ISODate('2026-10-31T17:00:00Z')}}},
 {$unwind:'$items'},
 {$group:{_id:'$items.productId',quantity:{$sum:'$items.quantity'},revenue:{$sum:{$multiply:['$items.unitPrice','$items.quantity']}}}},
 {$sort:{quantity:-1,revenue:-1,_id:1}},{$limit:5}
]));
