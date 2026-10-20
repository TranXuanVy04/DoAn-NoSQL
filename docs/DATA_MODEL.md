# Thiết kế MongoDB và dữ liệu mẫu

## Vì sao MongoDB phù hợp đa ngành

Các sản phẩm chia sẻ SKU, tên, danh mục, giá, tồn kho nhưng thuộc tính riêng khác nhau: điện tử có hãng/bảo hành/kết nối; thời trang có size/chất liệu/màu; gia dụng có xuất xứ/dung tích. Một collection với `kind` + `attributes` cho phép cùng truy vấn catalog mà không tạo nhiều cột rỗng hoặc nhiều bảng nối thuộc tính. Khi thêm ngành mới cần bổ sung rule validator/form, không phải thay tất cả document cũ.

MongoDB linh hoạt không có nghĩa bỏ kiểm soát dữ liệu. `tools/validators.json` khai báo `$jsonSchema` và `oneOf` theo ngành để từ chối giá sai kiểu, thiếu thuộc tính bắt buộc hoặc trạng thái sai. Service cũng kiểm tra input để trả thông báo cho form.

## Collections

| Collection | Field chính | Kiểu / mục đích |
|---|---|---|
| danhmuc | _id, name, kind | ObjectId, string, enum ngành |
| sanpham | _id, sku, name, categoryId, kind, price, stock, isActive, attributes | ObjectId; Int64 tiền; Int32 kho; BSON document thuộc tính |
| hoadon | _id, number, checkoutKey, customerToken, customer, createdAt, status, items, total | Khách và các dòng đơn được nhúng, giá lưu tại lúc mua |

Ví dụ ý nghĩa (trong DB các chuỗi id/ngày bên dưới là ObjectId/Date thật):

```javascript
// Cùng collection sanpham
{ _id: ObjectId("000000000000000000000064"), kind: "electronics",
  name: "Tai nghe EC 01", price: NumberLong(115000),
  attributes: { brand: "EC Tech", warrantyMonths: 12, connectivity: "USB-C" } }
{ _id: ObjectId("00000000000000000000008c"), kind: "fashion",
  name: "Áo thun EC 01", price: NumberLong(215000),
  attributes: { size: "S", material: "Cotton", color: "Đen" } }
```

Đơn có `items`:

```javascript
{ productId: ObjectId("000000000000000000000064"), sku: "ELECTRONICS-001",
  name: "Tai nghe EC 01", categoryName: "Điện tử", kind: "electronics",
  attributes: { brand: "EC Tech", warrantyMonths: 12, connectivity: "USB-C" },
  unitPrice: NumberLong(105000), quantity: 1, lineTotal: NumberLong(105000) }
```

Đơn đọc trực tiếp `items`, không JOIN/`$lookup` về sanpham. Đổi giá/tên/thuộc tính sản phẩm không thay đổi đơn cũ. Danh mục tên cũng được snapshot. `productId` vẫn giữ để đối chiếu, nhóm Top 5 và hoàn kho khi hủy.

## Embedded documents và đánh đổi

- Đọc một đơn lấy được khách, dòng hàng, thuộc tính và giá lịch sử trong một document.
- Chấp nhận trùng dữ liệu vì lịch sử mua phải độc lập với catalog hiện tại.
- Không sửa toàn bộ đơn cũ khi đổi tên hàng. Nếu sửa thông tin đơn phải có quy tắc nghiệp vụ riêng.
- Đơn lớn bị giới hạn 50 dòng ở app; dữ liệu mẫu có đúng 2–5 dòng/đơn. Giới hạn này giúp tránh document quá lớn; MongoDB vẫn có giới hạn kích thước BSON document.
- Trừ kho nằm ở sanpham, nên cần transaction đa document với replica set. Không suy ra đơn nhúng thì mọi thao tác tự động atomic.
- Ngừng bán giữ productId cho hoàn kho; đơn vẫn đọc được từ snapshot.

## Dữ liệu và nạp lại

`generate_data.py` deterministic, ID cố định, 40 sản phẩm/ngành. 72 đơn có nhiều ngày và trạng thái; Top 5 có hàng lặp. Tiền Int64, ngày Date, ID ObjectId được ghi trong **Extended JSON canonical**. Không dùng JSON thường rồi kỳ vọng Compass tự đoán kiểu long/date.

`seed.py`: chỉ cho DB `_dev`, `_test`, `_bench`; kiểm tra DB trống; tạo validator/index trước; insert cả bộ trong transaction. Chạy lại dữ liệu seed nguyên vẹn báo NO-OP, DB đã chỉnh sửa thì dừng. Không có cờ reset/drop gây mất dữ liệu.

Validator xác nhận kiểu/required/range nhưng không tự kiểm tra được mọi bất biến như total bằng tổng dòng; service và bộ test chịu trách nhiệm đối chiếu các phép tính đó.

Nguồn kỹ thuật: https://www.mongodb.com/docs/manual/data-modeling/design-patterns/polymorphic-data/polymorphic-schema-pattern/ ; https://www.mongodb.com/docs/manual/core/schema-validation/ ; https://www.mongodb.com/docs/drivers/csharp/current/crud/transactions/
