# Bắt đầu từ đây

**Lần đầu chạy:** mở `README.md`, làm lần lượt phần 1 → 4. Mọi lệnh chạy từ thư mục có README, không phải từ Views hay Controllers.

## Hai cách chạy web

| Cách | Dành cho | Lệnh sau khi MongoDB đã seed và đã đặt mật khẩu Admin |
|---|---|---|
| Chạy source | Vỹ/Tín/Tuấn muốn sửa code | `dotnet run --project src/Ecommerce.Web -c Release --urls http://localhost:5080` |
| Chạy bản đã biên dịch | Demo nhanh, đã cài .NET10 | Vào `publish`, chạy `dotnet Ecommerce.Web.dll --urls http://localhost:5080` |

Với cả hai cách, đặt `$env:ASPNETCORE_ENVIRONMENT="Development"` trong PowerShell trước khi chạy để dùng User Secrets đã đặt theo README. Lệnh **dotnet Ecommerce.Web.dll** dùng assembly .NET portable; chạy thực tế Windows cần được nhóm xác nhận trên máy của mình.

Không cần cấu hình hai server frontend/backend riêng: Razor và controller chạy chung tại http://localhost:5080. MongoDB chạy riêng tại 27017. Quản trị tại `/Account/Login`, username **admin**, password do bạn đặt trong User Secrets.

Để sửa mã, dùng source. Không sửa file DLL. Sau khi sửa, build/publish lại; bản publish đi kèm chỉ tương ứng source tại thời điểm bàn giao.

## Gợi ý thứ tự đọc

1. README.md — cài, seed, chạy web/Compass.
2. docs/HANDOFF.md — 9 task Vỹ và phần còn cần nhóm.
3. docs/CONTRACTS.md — chốt với Tín/Tuấn.
4. docs/COMPASS.md và docs/BENCHMARK.md — phần DB Tester.
5. docs/DEMO.md — bảo vệ.

## Nếu chưa thấy dữ liệu

Kiểm tra MongoDB rs0, DB ecommerce_dev và bước seed. Các collection đúng là `danhmuc`, `sanpham`, `hoadon`. Dashboard seed chọn từ **2026-08-01 đến 2026-10-31**. Đơn mới phải Completed và nằm trong khoảng ngày đang lọc mới được tính doanh thu.
