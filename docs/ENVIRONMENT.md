# Thiết lập môi trường

Cài .NET SDK 10, Node.js 24 và MongoDB Shell (mongosh). Dùng MongoDB Atlas hoặc server MongoDB replica set do nhóm quản lý.

## Database dùng chung

Trên Atlas: tạo cluster, tạo tài khoản database riêng cho từng thành viên và cho phép IP các máy trong Network Access. Cả nhóm dùng cùng database ecommerce_dev. Tên database CI/test phải tách biệt.
Connection string từ Atlas có dạng mongodb+srv://...; không commit URI chứa mật khẩu. Backend mặc định để trống URI và sẽ báo thiếu cấu hình cho đến khi bạn đặt User Secrets/environment.

Chạy từ root repo:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet user-secrets set 'Mongo:ConnectionString' '<URI-cua-ban>' --project src/Ecommerce.Web
dotnet user-secrets set 'Mongo:Database' 'ecommerce_dev' --project src/Ecommerce.Web
dotnet user-secrets set 'Admin:Username' 'admin' --project src/Ecommerce.Web
dotnet user-secrets set 'Admin:Password' '<mat-khau-rieng-it-nhat-12-ky-tu>' --project src/Ecommerce.Web
```

Hoặc đặt Mongo__ConnectionString, Mongo__Database, Admin__Username, Admin__Password trong biến môi trường. Backend đã tích hợp DotNetEnv 3.2.0 để đọc src/Ecommerce.Web/.env trong Development. File env được đọc theo ContentRootPath (cả Visual Studio và dotnet run), không nạp trong Testing/Production và không được copy vào build/publish. Giá trị env file ghi đè appsettings/User Secrets; biến môi trường hệ điều hành và command line được ưu tiên cao hơn. ASPNETCORE_ENVIRONMENT phải đặt bằng launchSettings/terminal, không đặt trong .env vì môi trường đã được chọn trước khi nạp file.

## Nạp dữ liệu ban đầu

Một thành viên thực hiện trên DB development trống. Biến MONGO_URI/MONGO_DB dành cho mongosh, khác với biến cấu hình .NET.

```powershell
$env:MONGO_DB='ecommerce_dev'
# Nhập URI vào biến MONGO_URI bằng cấu hình môi trường riêng, không lưu vào source.
mongosh "$env:MONGO_URI" --file database/seed.mongosh.js
```

Dùng URI có database hoặc quyền truy cập ecommerce_dev phù hợp. Script từ chối ghi đè database có dữ liệu khác, nạp lại seed giống nhau là no-op. Seed dùng transaction nên cần replica set/Atlas. Không chạy seed sau khi nhóm đã sửa dữ liệu trừ khi cố ý dùng database development mới.

## Chạy ứng dụng

```powershell
dotnet run --project src/Ecommerce.Web --urls http://localhost:5080
```

Terminal khác:

```powershell
cd src/storefront
npm ci
Copy-Item .env.local.example .env.local
npm run dev
```

Storefront http://localhost:3000; MVC/API http://localhost:5080; Admin login tại /Account/Login. Mock mặc định true, catalog mock không cần backend. Đổi NEXT_PUBLIC_USE_MOCK_API=false và restart Next.js để gọi API thật.
CORS chỉ cho origin cụ thể trong Cors:AllowedOrigins; browser gửi credentials include. Không đặt connection string/mật khẩu trong biến NEXT_PUBLIC_. Next.js checkout/Admin auth còn chưa triển khai.

## Dùng file .env cho backend

Mở src/Ecommerce.Web/.env, điền Mongo__ConnectionString bằng URI Atlas mới, Mongo__Database=ecommerce_dev và Admin__Password bằng mật khẩu riêng ít nhất 12 ký tự. Không đặt URI trong frontend. File .env đã được Git ignore. Restart backend sau mỗi lần đổi env. File .env.example là mẫu không chứa secrets.
