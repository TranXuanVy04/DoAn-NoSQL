# E-commerce đa ngành hàng

ASP.NET Core .NET 10 + MongoDB Driver 3.12, Next.js 16 + React + TypeScript.
Backend hiện có MVC hoàn chỉnh và API GET catalog/analytics. Next.js hiện có catalog skeleton; checkout/Admin mới nằm trong task board.

## Bắt đầu

Xem [hướng dẫn môi trường](docs/ENVIRONMENT.md). Cài .NET 10, Node.js 24, MongoDB Shell và dùng MongoDB Atlas hoặc MongoDB replica set đã có.
Connection string lưu bằng User Secrets hoặc biến môi trường. Ba máy muốn dùng chung dữ liệu cần cùng cluster và database; nhóm thống nhất một người nạp seed.

## Cấu trúc

```text
Ecommerce.slnx              solution chứa projects và file repo
src/Ecommerce.Web/         MVC + API + Mongo services
src/storefront/            Next.js, mock và Node tests
contracts/                 OpenAPI và JSON examples
database/                  mongosh seed, index, explain, validator
data/                      3 danh mục, 120 sản phẩm, 72 hóa đơn
tests/Ecommerce.UnitTests/  xUnit và HTTP tests với fake
docs/                      kiến trúc, workflow, task board
templates/                 mẫu tái sử dụng
.github/                   CI và PR/issue templates
```

## Kiểm tra

```powershell
dotnet test Ecommerce.slnx -c Release
cd src/storefront
npm ci
npm run contract:types
npm run typecheck
npm run lint
npm test
npm run build
```

- [Phân công](docs/TASK_BOARD.md), [workflow](docs/TEAM_WORKFLOW.md), [kiến trúc](docs/ARCHITECTURE.md).
- [MongoDB](docs/MONGODB_SCHEMA.md), [Compass](docs/COMPASS.md), [dữ liệu mẫu](docs/DUMMY_DATA.md).
- [Kết quả kiểm tra và giới hạn](docs/TEST_REPORT.md).

Giá là VND nguyên BSON Int64. Checkout cần transaction; không dùng MongoDB standalone. Doanh thu hiện tính trạng thái Completed theo snapshot; đây chưa phải tích hợp cổng thanh toán.
