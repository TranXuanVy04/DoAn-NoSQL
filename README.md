# E-commerce đa ngành hàng

ASP.NET Core MVC + Razor Views + Web API, .NET 10, MongoDB Driver 3.12.0 và MongoDB Atlas.
Giao diện và API chạy chung một ứng dụng; không cần server giao diện riêng.

## Chạy dự án

Cấu hình src/Ecommerce.Web/.env theo [ENVIRONMENT](docs/ENVIRONMENT.md), sau đó:

```powershell
dotnet run --project src/Ecommerce.Web
```

Visual Studio: mở Ecommerce.slnx, chọn Ecommerce.Web làm Startup Project rồi F5.
Catalog tại /Catalog, giỏ hàng /Cart, đăng nhập quản trị /Account/Login. Các API GET tại /api/categories, /api/products và /api/analytics/ (yêu cầu đăng nhập).

## Cấu trúc

- src/Ecommerce.Web/: Controllers, Razor Views, Models, Services, Repositories và Modules API.
- tests/Ecommerce.UnitTests/: xUnit và HTTP tests bằng fake.
- tests/data/: kiểm tra JSON dataset/contract độc lập bằng Node.js, không cần cài package.
- contracts/: OpenAPI và JSON examples.
- database/: mongosh seed, validator, indexes và explain.
- data/: 3 danh mục, 120 sản phẩm, 72 hóa đơn.
- docs/, templates/: tài liệu nhóm và mẫu code.

## Kiểm tra

```powershell
dotnet test tests/Ecommerce.UnitTests -c Release
# Tùy chọn, cần Node.js 24 để kiểm tra dataset/contract:
node --test tests/data/*.test.mjs
```

Tiền VND nguyên BSON Int64; checkout dùng transaction trên Atlas/replica set. Doanh thu hiện tính đơn Completed theo snapshot; chưa tích hợp cổng thanh toán.
Xem [TASK_BOARD](docs/TASK_BOARD.md) và [TEST_REPORT](docs/TEST_REPORT.md) cho trạng thái thực tế.
