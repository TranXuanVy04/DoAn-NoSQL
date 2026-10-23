# Quản lý sản phẩm và biến thể

Vào /Admin/Products → Thêm sản phẩm. Nhập SKU chung, tên, danh mục và ảnh. Chọn Có biến thể rồi thêm từng dòng: SKU, màu, kích cỡ, giá, tồn kho và trạng thái bán. Thuộc tính chung nhập JSON, ví dụ {"material":"Cotton"}; màu và size nằm trong lựa chọn từng biến thể. Các nhóm khác nhập ở Thuộc tính lựa chọn khác, ví dụ {"capacity":"256GB"}. Tất cả biến thể của một sản phẩm cần cùng nhóm thuộc tính.

Sản phẩm không biến thể dùng giá và tồn kho chung. Sản phẩm có biến thể chỉ lưu giá/tồn kho ở từng biến thể. Không đổi loại sau khi tạo, để đơn cũ hoàn đúng kho. Giữ nguyên ID biến thể đã lưu; muốn bỏ bán thì tắt Đang bán. Dòng mới chưa lưu có thể xóa.

Mỗi lần lưu hoặc thay đổi tồn kho, revision tăng. Nếu khách mua trong lúc quản trị sửa, form cũ bị từ chối và yêu cầu tải lại. Giỏ hàng phân biệt productId + variantId, hóa đơn lưu bản chụp lựa chọn và giá lúc mua; hủy đơn hoàn đúng biến thể trong transaction.

## Chuẩn bị Atlas

Nếu validator/index chưa cập nhật, mở mongosh kết nối Atlas, từ thư mục gốc repo chạy:

```javascript
load('data/setup-variants.mongosh.js')
```

Script nhắm ecommerce_dev, không xóa dữ liệu. Nếu dùng database khác, sửa tên trước khi chạy. Index SKU chung vẫn giữ; index unique variants.sku bảo vệ SKU biến thể giữa các sản phẩm. Backend kiểm tra thêm SKU và tổ hợp lựa chọn bị trùng. Các ID danh mục gốc và quy tắc thuộc tính được khai báo trong CategoryAttributeRules ở appsettings.json.

## Demo

1. Tạo danh mục cha/con; thử chọn cha gây vòng lặp và thử xóa danh mục còn sản phẩm.
2. Tạo sản phẩm thường, sửa ảnh/giá/tồn, ngừng bán.
3. Tạo áo có Đen/M và Trắng/L. Đặt hàng thử cả hai biến thể trong cùng giỏ.
4. Kiểm tra giỏ và checkout hiển thị đúng lựa chọn. Thử số lượng vượt tồn kho.
5. Sau đặt hàng, kiểm tra items trong hóa đơn và tồn kho biến thể; đổi giá sản phẩm và xác nhận đơn cũ không đổi.
6. Hủy đơn Pending, kiểm tra hoàn đúng tồn kho. Hủy lại không hoàn kho lần hai.

Kiểm thử tự động dùng dịch vụ bộ nhớ cho luồng MVC và kiểm thử trực tiếp bộ chuyển form/snapshot. Ghi đơn và transaction MongoDB cần demo trên Atlas; không tự ghi dữ liệu thử lên Atlas.
