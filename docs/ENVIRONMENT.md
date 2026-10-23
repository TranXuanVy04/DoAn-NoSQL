# Thiết lập môi trường

Cài .NET SDK 10 và MongoDB Shell (mongosh). Node.js 24 chỉ cần nếu chạy kiểm tra dataset tùy chọn. Dùng MongoDB Atlas hoặc server MongoDB replica set do nhóm quản lý.

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

Giao diện Razor và Web API cùng chạy trên địa chỉ backend; Admin đăng nhập tại /Account/Login. Không cần chạy thêm giao diện riêng.

## Dùng file .env cho backend

Mở src/Ecommerce.Web/.env, điền Mongo__ConnectionString bằng URI Atlas mới, Mongo__Database=ecommerce_dev và Admin__Password bằng mật khẩu riêng ít nhất 12 ký tự. Không đưa URI vào trình duyệt hoặc View. File .env đã được Git ignore. Restart backend sau mỗi lần đổi env. File .env.example là mẫu không chứa secrets.
