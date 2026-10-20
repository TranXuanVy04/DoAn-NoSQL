# Compass: thao tác để bảo vệ và import/export

## Xem cấu trúc đa hình

1. Connect URI trong README, mở ecommerce_dev → sanpham → Documents.
2. Filter `{ kind: "electronics" }`, xem `attributes.brand`, `warrantyMonths`, `connectivity`.
3. Đổi filter `{ kind: "fashion" }`, xem `attributes.size`, `material`, `color`.
4. Filter `{ kind: "home" }`, xem material/origin/capacityLiters.
5. Chỉ rõ vẫn là **cùng collection sanpham**. Chuyển JSON/List/Table view tùy phiên bản Compass để chỉ trường; không cần chỉnh dữ liệu.

Đếm qua mongosh tích hợp hoặc `mongosh` riêng:

```javascript
use ecommerce_dev
db.sanpham.countDocuments({})   // 120 sau seed, trước khi tạo thêm
db.hoadon.countDocuments({})    // 72 sau seed, trước khi đặt thêm
db.sanpham.aggregate([{ $group: { _id: "$kind", count: { $sum: 1 } } }])
db.hoadon.aggregate([
 { $project: { n: { $size: "$items" } } },
 { $group: { _id: null, min: { $min: "$n" }, max: { $max: "$n" } } }
])
```

Các đơn seed có 2–5 dòng; đơn đặt thực có thể có 1–50 dòng theo contract.

## Chứng minh snapshot của đơn vừa mua

1. Đặt hai sản phẩm trên web, ghi mã EC-... trên trang kết quả.
2. Mở hoadon, filter `{ number: "MÃ_ĐƠN_VỪA_TẠO" }`.
3. Mở `items`, chỉ name, sku, attributes, unitPrice, quantity, lineTotal và total.
4. SV1 sửa giá/thuộc tính sản phẩm trên Admin. Reload hoadon: các giá trị snapshot cũ không đổi.
5. Không thay `items` trực tiếp; dữ liệu lịch sử phải được giữ.

## Import file mẫu có sẵn

Cách khuyến nghị giữ validator/index: chạy `tools/seed.py` như README. Để thực hành import tay, dùng **DB mới kết thúc `_test`**, không import đè ecommerce_dev:

1. Tạo DB `compass_manual_test`, collection `danhmuc`.
2. Trong Documents → **Add Data → Import JSON or CSV file** (tên nút có thể khác theo bản Compass), chọn `data/danhmuc.json`, loại JSON.
3. Tạo `sanpham`, import `data/sanpham.json`. Tạo `hoadon`, import `data/hoadon.json`.
4. File là một mảng Extended JSON canonical; nếu hộp thoại có lựa chọn JSON array/JSON lines, chọn array.
5. Kiểm tra ObjectId ở `_id`/categoryId/productId, Long ở price/unitPrice/total và Date ở createdAt. Không được chỉ thấy chuỗi `$oid` hay object `$numberLong` trong document.
6. Xác nhận counts 3/120/72. Compass import document không tự chép toàn bộ index/validator; collection mới import tay dùng cho thực hành, chưa đủ cấu hình vận hành web.

## Export và import lại

Trong từng collection, dùng **Export Data**, chọn **toàn bộ collection** (không chỉ kết quả filter), định dạng **JSON/Extended JSON**, ưu tiên canonical nếu hộp thoại hỗ trợ. CSV không bảo toàn mảng nhúng và kiểu BSON như yêu cầu này. Đặt các file vào thư mục riêng, import vào DB test trống rồi kiểm tra lại count và BSON.

Để kiểm chứng định lượng, dùng script roundtrip (tên đích/thư mục phải mới):

```powershell
.\.venv\Scripts\python.exe tools/transfer_data.py roundtrip --db ecommerce_dev --target compass_roundtrip_test --folder exports/roundtrip01
```

Script export/import canonical bằng driver, sau đó so sánh **toàn bộ document**, không chỉ count. Kết quả PASS chứng minh roundtrip qua script, **không thay thế bằng chứng bạn đã bấm Import/Export trong giao diện Compass**.

Để kiểm tra sau thao tác Compass, export lại DB đã import bằng script rồi so với DB nguồn bằng công cụ Python/Compass. Không đánh dấu ca Compass thủ công PASS nếu chưa thao tác. Xem `docs/TESTING.md` để ghi bằng chứng.

Nếu import sai kiểu, dừng demo và dùng DB test mới; không sửa hàng loạt dữ liệu demo mà chưa có bản export.

Nguồn: https://www.mongodb.com/docs/compass/current/import-export/ ; https://www.mongodb.com/docs/manual/reference/mongodb-extended-json/
