# Kiến trúc và boundaries

Modular monolith trong `src/Ecommerce.Web`: module mới đặt trong `Modules/<Feature>/`; MVC cũ giữ nguyên Controllers/Services/Models/Views.

```text
contracts/openapi/          nguồn contract, review chung
src/Ecommerce.Web/
  Modules/Catalog/         adapter API đọc Categories/Products (SV1)
  Modules/Analytics/       adapter báo cáo có auth (SV2)
  Services/               interface + Mongo implementation đang dùng
  Models/                 BSON/domain hiện tại; thay đổi cần review
src/storefront/src/
  app/                    Next.js App Router
  features/catalog/       UI + service
  shared/api/             fetch client + generated types
  mocks/                  fake theo DTO contract
  features/<feature>/     module tiếp theo do SV3 tạo
contracts/examples/       fixtures dùng chung
 tests/                    xUnit; Node tests trong storefront
 database/                 mongosh seed/validator/index/explain
 templates/                ví dụ và hướng dẫn tái sử dụng
```

Frontend chỉ gọi service, không truy cập MongoDB. Chuyển mock bằng env, không sửa UI. Backend API chuyển BsonDocument sang JSON rõ ràng thay vì để serializer trả kiểu nội bộ driver.

Giữ ICatalogService/IOrderService/IDashboardService làm seam để phát triển độc lập. Không đăng ký fake trong Program.cs production. Không tạo generic repository hoặc microservice không cần thiết.

Contract mới đang là đề xuất. Các tên BSON cũ giữ nguyên; mapping tới schema master prompt là công việc migration có review. Tiền VND nguyên Int64 được giữ để bảo toàn dữ liệu, API giới hạn safe integer của JS. Pipeline revenue chỉ Completed; Dashboard hiển thị UTC+7, lưu UTC.
