# Kiến trúc

Modular monolith ASP.NET Core MVC + Razor Views + Web API, dùng chung MongoDB services.

- Controllers/ + Views/: giao diện catalog, giỏ hàng, checkout và Admin.
- Modules/Catalog/: JSON API đọc danh mục/sản phẩm.
- Modules/Analytics/: JSON API báo cáo có xác thực.
- Services/: kiểm tra nghiệp vụ, tạo snapshot và điều phối transaction.
- Repositories/: CategoryRepository, ProductRepository, OrderRepository và AnalyticsRepository chứa truy vấn MongoDB; dùng class cụ thể, không có interface Repository.
- Models/: BSON domain và form input/ViewModel.
- wwwroot/: CSS và tài nguyên giao diện.
- tests/: xUnit HTTP fake và kiểm tra dataset/contract độc lập.

Luồng dữ liệu: Controller → Service → Repository → MongoDB.

Razor controller trả View; API controller trả JSON. Cùng gọi service, không nhân đôi nghiệp vụ. JavaScript trong Razor có thể gọi API cùng origin, không cần CORS mặc định. Mọi thao tác ghi MVC giữ cookie auth/antiforgery.
Database không được truy cập trực tiếp từ trình duyệt. URI đọc từ .env chỉ trong Development, secrets không đưa vào View/API.
BSON giữ tên field hiện tại, giá VND nguyên Int64, timestamp UTC, báo cáo UTC+7.
