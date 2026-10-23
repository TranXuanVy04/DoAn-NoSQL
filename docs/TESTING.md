# Kiểm thử

Backend: dotnet test tests/Ecommerce.UnitTests -c Release. HTTP tests dùng fake, không cần Atlas.
Dataset/contract tùy chọn: node --test tests/data/*.test.mjs (Node.js 24, không cần npm install).

Cần bổ sung integration trên DB _test riêng: CRUD, snapshot sau sửa giá, retry/ownership, transaction rollback/concurrency stock, Completed revenue và explain.
Kiểm tra giao diện Razor trong browser: catalog/detail, cart/checkout, login/antiforgery và Admin dashboard. Fake HTTP tests không thay thế E2E thật.
