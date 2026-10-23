# Audit 21/10/2026

ASP.NET Core MVC net10.0, MongoDB.Driver 3.12.0; Cookie Auth, antiforgery, cart session, checkout transaction và snapshot. Có Razor Views, contract 18 operations và xUnit; chưa có CI workflow.
BSON dùng ObjectId/Int64/Date UTC; giá VND nguyên. Completed là trạng thái doanh thu hiện tại, chưa có payment provider.
Database thao tác qua mongosh/Compass và backend MongoDB Driver. Dữ liệu mẫu 3/120/72 được giữ nguyên. Connection string mặc định rỗng, phải cung cấp bằng User Secrets/environment; chưa có Atlas credential hoặc cluster nào được tạo trong lần chỉnh sửa này.
Model cũ còn thiếu description/images/createdAt/updatedAt trên Product, categoryId trong snapshot và updatedAt trên Order so với master prompt; cần migration có review.
Xem TEST_REPORT.md và TASK_BOARD.md cho kiểm chứng và công việc còn lại. Không thay đổi DB thật hoặc push/merge remote.
