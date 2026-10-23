# E-commerce đa ngành hàng

ASP.NET Core MVC + Razor Views, .NET 10, MongoDB Driver 3.12.0 và MongoDB Atlas.
Ứng dụng render HTML bằng Razor; Controller gọi Service và Repository trực tiếp.

## Chạy dự án

Cấu hình src/Ecommerce.Web/.env theo [ENVIRONMENT](docs/ENVIRONMENT.md), sau đó:

```powershell
dotnet run --project src/Ecommerce.Web
```

Visual Studio: mở Ecommerce.slnx, chọn Ecommerce.Web làm Startup Project rồi F5.
Catalog tại /Catalog, giỏ hàng /Cart, đăng nhập quản trị /Account/Login. Báo cáo tại /Admin/Dashboard (yêu cầu đăng nhập).

## Cấu trúc

- src/Ecommerce.Web/: Controllers, Razor Views, Models, Services, Repositories.
- tests/Ecommerce.UnitTests/: xUnit và HTTP tests bằng fake.
- tests/data/: kiểm tra JSON dataset độc lập bằng Node.js, không cần cài package.
- database/: mongosh seed, validator, indexes và explain.
- data/: 3 danh mục, 120 sản phẩm, 72 hóa đơn.
- docs/, templates/: tài liệu nhóm và mẫu code.

## Kiểm tra

```powershell
dotnet test tests/Ecommerce.UnitTests -c Release
# Tùy chọn, cần Node.js 24 để kiểm tra dataset:
node --test tests/data/*.test.mjs
```

Tiền VND nguyên BSON Int64; checkout dùng transaction trên Atlas/replica set. Doanh thu hiện tính đơn Completed theo snapshot; chưa tích hợp cổng thanh toán.
Xem [TASK_BOARD](docs/TASK_BOARD.md) và [TEST_REPORT](docs/TEST_REPORT.md) cho trạng thái thực tế.

Giao diện bán hàng dùng template Ogani do nhóm cung cấp. Xem docs/OGANI_TEMPLATE.md cho route, dữ liệu mock và cách thay ảnh/nội dung.
