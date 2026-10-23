# Template thực tế

- Razor page/layout: src/Ecommerce.Web/Views/Catalog/Index.cshtml và Views/Shared/_Layout.cshtml.
- MVC Controller: Controllers/AdminController.cs; form/ViewModel: Models/Forms.cs.
- API Controller: Modules/Catalog/ProductsController.cs.
- Interface/service/model: Services/ICatalogService.cs, IOrderService.cs, IDashboardService.cs, MongoCatalogService.cs và Models/Domain.cs.
- Repository cụ thể: Repositories/CategoryRepository.cs, ProductRepository.cs, OrderRepository.cs và AnalyticsRepository.cs (không có interface Repository).
- Read-model mẫu: backend/Feature.cs.txt.
- Fake và HTTP test: tests/Ecommerce.UnitTests/.
- JSON request/response mẫu: contracts/examples/.
- Mongo validator/seed/index/explain: database/; data/: Extended JSON mẫu.
- Feature specification: documentation/feature.md.

Copy mẫu, đổi namespace/feature, cập nhật contract và đăng ký DI khi cần. Fake không đăng ký production. Giữ cookie auth và antiforgery cho thao tác ghi từ Razor.
