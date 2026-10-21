{
// Run from repository root with mongosh. Defines indexes; never drops existing indexes.
const name = process.env.MONGO_DB || 'ecommerce_dev';
if (!/^[A-Za-z][A-Za-z0-9_]*_(dev|test|bench)$/.test(name)) throw new Error('Use a _dev, _test or _bench database.');
const target = db.getSiblingDB(name);
target.danhmuc.createIndex({name:1},{unique:true,name:'uq_category_name'});
target.sanpham.createIndex({sku:1},{unique:true,name:'uq_product_sku'});
target.sanpham.createIndex({categoryId:1,isActive:1,price:1},{name:'ix_catalog_category_active_price'});
target.hoadon.createIndex({checkoutKey:1},{unique:true,name:'uq_checkout_key'});
target.hoadon.createIndex({number:1},{unique:true,name:'uq_order_number'});
target.hoadon.createIndex({status:1,createdAt:1},{name:'ix_order_status_date'});
print('Indexes ready: ' + name);
}
