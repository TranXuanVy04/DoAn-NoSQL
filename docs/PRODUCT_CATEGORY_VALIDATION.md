# Kiểm tra thuộc tính sản phẩm theo danh mục gốc

Sản phẩm chỉ lưu categoryId và attributes, không còn kind. Backend đi theo parentId đến danh mục gốc rồi đọc CategoryAttributeRules trong appsettings.json. Mỗi ID danh mục gốc ánh xạ các trường bắt buộc và kiểu string hoặc nonNegativeNumber. Danh mục gốc mới chưa có cấu hình chỉ áp dụng kiểm tra JSON chung. Đổi tên danh mục không làm thay đổi quy tắc; chuyển nhánh danh mục sẽ dùng quy tắc gốc mới khi lưu sản phẩm.

Ba ID mẫu tương ứng Điện tử, Thời trang và Gia dụng đã được cấu hình. Nếu dùng ID khác trên Atlas, cập nhật các khóa ID trong CategoryAttributeRules. Validation thuộc tính theo ngành được thực hiện ở backend; MongoDB validator chỉ kiểm tra cấu trúc chung, không truy vấn collection danh mục. Ghi trực tiếp qua Compass không chạy validation nghiệp vụ này.

## Chuyển Atlas hiện có

Mở mongosh kết nối Atlas, từ thư mục gốc repo chạy:

```javascript
load('data/migrate-remove-kind.mongosh.js')
```

Script nhắm database ecommerce_dev: cập nhật validator sanpham/hoadon từ data/validators.json trước, sau đó xóa kind ở sản phẩm và items trong hóa đơn. Không đổi giá và thuộc tính snapshot. Nếu database có tên khác, sửa tên trong script trước khi chạy. Code vẫn đọc được document cũ còn kind để có thể chuyển đổi lần lượt.
