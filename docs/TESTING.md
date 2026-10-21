# Kiểm thử

Backend: dotnet test Ecommerce.slnx -c Release. HTTP tests dùng fake catalog/dashboard và Mongo URI test giả lập, không kết nối DB thật.
Frontend: npm ci, npm run contract:types, npm run typecheck, npm run lint, npm test, npm run build tại src/storefront.
Node tests kiểm tra dữ liệu seed, số tiền chính xác bằng BigInt, pipeline expected report và ranh giới contract.

Cần bổ sung integration thật với database _test riêng: CRUD, transaction rollback, checkout retry, cạnh tranh stock, snapshot sau đổi giá, ownership, Completed revenue và index explain. Không dùng DB nhóm/production cho test ghi.
Kiểm tra browser E2E và Compass phải ghi nhận sau khi thực hiện; HTTP fake không thay thế chúng.
