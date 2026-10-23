# Danh mục cha–con

Category dùng Id, Name và ParentId; không còn Kind. ParentId=null là danh mục gốc; giá trị ObjectId trỏ đến danh mục cha. Có thể thêm nhiều cấp và tên ngành bất kỳ.

Ví dụ request tạo danh mục gốc: {"name":"Sách","parentId":null}.
Ví dụ tạo danh mục con: {"name":"Sách lập trình","parentId":"ID_CUA_DANH_MUC_SACH"}.

Service kiểm tra cha tồn tại, không chọn chính mình/descendant và không tạo vòng lặp. Xóa bị chặn nếu còn danh mục con hoặc sản phẩm. Form Razor có dropdown chọn cha. Tên danh mục vẫn duy nhất toàn hệ thống theo unique index hiện có.
Các thao tác sửa cây đang kiểm tra ở service, chưa có khóa toàn cây chống hai Admin đồng thời tạo quan hệ vòng; cần giới hạn chỉnh cây đồng thời hoặc bổ sung cơ chế concurrency nếu triển khai thực tế.

## Atlas đã có dữ liệu

Validator cũ yêu cầu kind nên phải migration trước khi ghi bằng code mới. Code đọc bỏ qua field legacy để vẫn xem được dữ liệu cũ. Không import lại hoặc reset DB nhóm.
Chạy trong root repo, dùng tài khoản đủ quyền tạo backup, collMod, update và createIndex:

```powershell
$env:MONGO_DB='ecommerce_dev'
# Nhập URI Atlas vào biến atlasUri trên máy mình, không lưu vào source.
$atlasUri = Read-Host 'URI Atlas' -MaskInput
$env:APPLY_CATEGORY_MIGRATION='false'
mongosh "$atlasUri" --file database/migrate-category-parent.mongosh.js
# Đọc kế hoạch, xác nhận đúng database và dừng chỉnh danh mục trong lúc migration.
$env:APPLY_CATEGORY_MIGRATION='true'
mongosh "$atlasUri" --file database/migrate-category-parent.mongosh.js
$env:APPLY_CATEGORY_MIGRATION='false'
```

Script sao lưu document gốc vào danhmuc_before_parentid (không ghi đè backup đã có), cập nhật validator danh mục/sản phẩm, thêm parentId=null nếu thiếu, bỏ kind chỉ trong danh mục và tạo index parentId. Giữ nguyên IDs, liên kết sản phẩm, giá và toàn bộ hóa đơn/snapshot. Script chưa được thực thi lên Atlas trong lần sửa code này. collMod và cập nhật document là các bước riêng, cần chạy trong thời gian nhóm không sửa dữ liệu và giữ backup để phục hồi nếu có lỗi.

## Thuộc tính sản phẩm

Product/OrderItem.kind được giữ để tương thích dữ liệu/snapshot cũ; nó không còn xác định cha–con hoặc giới hạn danh mục. Sửa sản phẩm cũ giữ discriminator hiện tại và validation tương ứng. Sản phẩm mới dùng kind=general, attributes linh hoạt và vẫn kiểm tra key an toàn/kích thước, giá, stock. Validator sản phẩm cho phép kiểu mới ngoài ba kiểu mẫu. Bộ 120 sản phẩm mẫu vẫn thể hiện ba polymorphic schemas và 72 hóa đơn giữ snapshot nguyên vẹn.
