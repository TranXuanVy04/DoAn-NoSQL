# Templates

Copy mẫu, đổi feature/namespace và cập nhật contract trước đăng ký DI. PascalCase cho C#, camelCase cho TS/JSON.

- Backend controller: src/Ecommerce.Web/Modules/Catalog/CatalogApiController.cs.
- Interface/service/model/validation: Services/Contracts.cs, MongoCatalogService.cs, Models/Domain.cs và Forms.cs.
- Repository/read-model mẫu: backend/Feature.cs.txt.
- Unit/HTTP test: tests/Ecommerce.UnitTests/.
- Feature/component/page/service/client/types: src/storefront/src/.
- Hook: frontend/useProducts.ts.txt.
- Mock và fixtures: src/storefront/src/mocks/ và contracts/examples/.
- Mongo validator/seed/index/explain: database/; Extended JSON mẫu: data/.
- Feature specification: documentation/feature.md; PR/bug: .github/.

Xem docs/COMPASS.md và BENCHMARK.md cho import/export/query plan. Component tests/MSW và Mongo integration còn cần triển khai. Không đăng ký fake production.
