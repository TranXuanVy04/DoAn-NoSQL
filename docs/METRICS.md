# Quy tắc doanh thu và Top 5

Đây là quy tắc của bản khởi tạo, cần SV1/SV2 xác nhận trước merge.

- Chỉ `status == "Completed"` được tính.
- Dùng thời điểm **createdAt** của đơn để phân kỳ, không phải ngày chuyển Completed.
- `from` bắt đầu 00:00:00 UTC+7, `to` bao gồm hết ngày. Mongo nhận khoảng UTC **[from - 7h, (to + 1 ngày) - 7h)**; không dùng 23:59:59 dễ thiếu mili giây.
- VND nguyên, không phí giao hàng/thuế/khuyến mãi. `lineTotal=unitPrice*quantity`, `total=Σ lineTotal`.
- Doanh thu ngày/tháng = Σ total; số đơn = số Completed thỏa khoảng; AOV = doanh thu / số đơn, hiển thị làm tròn xuống VND. Không có đơn thì 0.
- Top 5 = nhóm `items.productId`, tổng quantity giảm dần; hòa theo doanh thu giảm dần; tiếp theo ObjectId tăng dần.
- Tên Top 5 lấy tên trong snapshot của đơn gần nhất (sort createdAt giảm, _id giảm). Không lookup sản phẩm; cùng productId đổi tên vẫn được nhóm chung.
- Biểu đồ chỉ có kỳ có dữ liệu, không tự tạo dòng 0; bảng bên dưới chứa số chính xác.

## Pipeline doanh thu

`$match` status + khoảng UTC → `$group` theo `$dateToString` timezone +07:00, sum total và count → `$sort` kỳ tăng.

## Pipeline Top 5

`$match` → `$sort` để tên snapshot ổn định → `$unwind: "$items"` → `$group` productId, sum quantity và sum unitPrice×quantity → `$sort` quantity/revenue/id → `$limit: 5`.

Pipeline C#: `Repositories/AnalyticsRepository.cs`; `Services/MongoDashboardService.cs` kiểm tra đầu vào rồi gọi repository. Query plan tham khảo: `database/explain.mongosh.js`. Node tests đối chiếu fixture với phép cộng độc lập từ snapshot; kiểm chứng DB/HTML thật cần chạy lại trên database test.

## Index

- `sanpham`: `{categoryId:1, isActive:1, price:1}`.
- `hoadon`: `{status:1, createdAt:1}`.
- Unique: category name, product SKU, checkoutKey và order number.

Index order giúp bước match chọn status/date. Unwind/group vẫn phải xử lý các dòng khớp; không hứa index loại bỏ chi phí group hay luôn làm nhanh hơn khi tập dữ liệu rất nhỏ hoặc gần như toàn bộ đơn khớp.

Nguồn: https://www.mongodb.com/docs/manual/aggregation/ ; https://www.mongodb.com/docs/manual/core/index-compound/ ; https://www.mongodb.com/docs/manual/reference/operator/aggregation/dateToString/
