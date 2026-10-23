// Chạy từ thư mục gốc repo trong mongosh đã kết nối Atlas.
// Không xóa document; cập nhật validator, bổ sung trường mặc định và tạo index.
const target = db.getSiblingDB('ecommerce_dev');
const validators = JSON.parse(require('fs').readFileSync('data/validators.json', 'utf8'));
for (const name of ['sanpham', 'hoadon']) {
    const result = target.runCommand({collMod: name, validator: validators[name], validationLevel: 'strict', validationAction: 'error'});
    if (result.ok !== 1) throw new Error(JSON.stringify(result));
}
printjson(target.sanpham.updateMany({variants: {$exists: false}}, {$set: {variants: []}}));
printjson(target.sanpham.updateMany({revision: {$exists: false}}, {$set: {revision: NumberLong(0)}}));
const index = target.sanpham.getIndexes().find(i => i.key['variants.sku'] === 1 && Object.keys(i.key).length === 1 && i.unique);
if (!index) target.sanpham.createIndex({'variants.sku': 1}, {name: 'variants_sku_unique', unique: true, partialFilterExpression: {'variants.sku': {$type: 'string'}}});
print('Đã cập nhật cấu trúc biến thể và index.');
