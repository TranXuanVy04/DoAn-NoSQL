# Chạy nhanh

Làm theo [docs/ENVIRONMENT.md](docs/ENVIRONMENT.md) để cấu hình MongoDB bằng User Secrets, nạp dữ liệu qua mongosh và chạy backend/frontend.

Backend: dotnet run --project src/Ecommerce.Web --urls http://localhost:5080.
Frontend: cd src/storefront, npm ci, npm run dev.

Database ecommerce_dev gồm danhmuc, sanpham, hoadon. Dashboard seed lọc 2026-08-01 đến 2026-10-31; chỉ Completed được tính doanh thu.
