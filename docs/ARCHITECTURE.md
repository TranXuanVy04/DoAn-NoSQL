# Kiến trúc

Modular monolith ASP.NET Core MVC + Razor Views, dùng chung MongoDB services.

- Controllers/ + Views/: giao diện catalog, giỏ hàng, checkout và Admin.
- Services/: kiểm tra nghiệp vụ, tạo snapshot và điều phối transaction.
- Repositories/: CategoryRepository, ProductRepository, OrderRepository và AnalyticsRepository chứa truy vấn MongoDB; dùng class cụ thể, không có interface Repository.
- Models/: BSON domain và form input/ViewModel.
- wwwroot/: CSS và tài nguyên giao diện.
- tests/: xUnit HTTP fake và kiểm tra dataset độc lập.

Luồng dữ liệu: Controller → Service → Repository → MongoDB.

Controller trả Razor View khi đọc và nhận form POST khi ghi. Không còn endpoint /api hoặc OpenAPI. Mọi thao tác ghi MVC giữ cookie auth/antiforgery.
Database không được truy cập trực tiếp từ trình duyệt. URI đọc từ .env chỉ trong Development, secrets không đưa vào View.
BSON giữ tên field hiện tại, giá VND nguyên Int64, timestamp UTC, báo cáo UTC+7.

Danh mục đã chuyển sang ParentId, xem CATEGORY_HIERARCHY.md cho quan hệ cha–con và migration Atlas. Kind còn lại trên sản phẩm/snapshot là discriminator tương thích, không phải trường danh mục.
