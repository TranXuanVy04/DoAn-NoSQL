# EC Market — bộ dự án SV3 của Vỹ

ASP.NET Core MVC **.NET 10**, giao diện **Razor View trong cùng project**, **MongoDB**. Không cần Node.js hay frontend riêng. Mở **thư mục gốc chứa README này** trong VS Code để chạy các lệnh bên dưới.

Gói gồm phần công việc SV3 theo Jira: EC-12, EC-13, EC-14, EC-21, EC-22, EC-23, EC-30, EC-31, EC-32. Vì chưa có repo/core chung, đã cung cấp service nền chạy thật để giao diện hoạt động; Tín/Tuấn có thể tiếp quản/thay các implementation qua interface trong `Services/Contracts.cs`. Đây là contract đề xuất của bản khởi tạo, chưa phải contract cả nhóm đã duyệt.

## 1. Chuẩn bị trên Windows

Cài:

1. **.NET SDK 10** (không chỉ Runtime): https://dotnet.microsoft.com/download/dotnet/10.0
2. **Python 3.12**: https://www.python.org/downloads/ (bật Add Python to PATH).
3. **Docker Desktop** với Linux containers, mở Docker Desktop và chờ Engine running: https://www.docker.com/products/docker-desktop/
4. **MongoDB Compass** để kiểm tra DB: https://www.mongodb.com/try/download/compass
5. Git và VS Code để làm việc nhóm. Máy phải còn trống cổng **27017, 5080**; dừng MongoDB local khác nếu đang chiếm 27017.

Kiểm tra trong terminal PowerShell:

```powershell
dotnet --version
py -3.12 --version
docker compose version
```

## 2. Tạo môi trường Python và khởi động MongoDB

Mở Terminal ở thư mục `Ecommerce-SV3` đã giải nén:

```powershell
py -3.12 -m venv .venv
.\.venv\Scripts\python.exe -m pip install -r tools/requirements.txt
docker compose up -d --wait
.\.venv\Scripts\python.exe tools/init_mongo.py
.\.venv\Scripts\python.exe tools/generate_data.py
.\.venv\Scripts\python.exe tools/seed.py --db ecommerce_dev
```

Mongod chạy **replica set một node rs0** để hỗ trợ transaction. Không dùng MongoDB standalone cho checkout. Nếu đã có MongoDB replica set local cùng cấu hình thì có thể dùng nó thay Docker; không chạy hai server cùng cổng.

Seed tạo **3 danh mục, 120 sản phẩm (40/ngành), 72 đơn**, mỗi đơn 2–5 dòng. Có Completed/Pending/Cancelled, ngày rải từ tháng 8 đến tháng 10/2026, sản phẩm mua lặp và giá lịch sử khác giá hiện tại. Đây là dữ liệu giả, không phải giao dịch thực.

Chạy lại seed trên DB chưa sửa sẽ báo **NO-OP**. Nếu DB đã có dữ liệu khác hoặc đã thao tác web, script từ chối ghi đè. Không drop DB chỉ để chạy lại; dùng DB dev/test mới và đổi cấu hình cho cả web lẫn công cụ. `generate_data.py` chỉ tạo file, không ghi DB.

## 3. Cấu hình Admin và chạy web

Thay chuỗi `MAT_KHAU_RIENG_IT_NHAT_12_KY_TU` bằng mật khẩu local riêng, ít nhất 12 ký tự. Không đưa mật khẩu lên Git:

```powershell
dotnet user-secrets set "Admin:Password" "MAT_KHAU_RIENG_IT_NHAT_12_KY_TU" --project src/Ecommerce.Web
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet build src/Ecommerce.Web -c Release
dotnet run --project src/Ecommerce.Web -c Release --no-build --urls http://localhost:5080
```

Giữ terminal chạy web mở. Tài khoản Admin là **admin**, mật khẩu là giá trị vừa đặt. User Secrets được nạp trong Development. Ứng dụng không có mật khẩu mặc định tự đăng nhập được.

| Trang | URL |
|---|---|
| Catalog khách hàng | http://localhost:5080 |
| Giỏ hàng | http://localhost:5080/Cart |
| Đăng nhập Admin | http://localhost:5080/Account/Login |
| Sản phẩm | http://localhost:5080/Admin/Products |
| Danh mục | http://localhost:5080/Admin/Categories |
| Đơn hàng | http://localhost:5080/Admin/Orders |
| Dashboard dữ liệu seed | http://localhost:5080/Admin/Dashboard?from=2026-08-01&to=2026-10-31 |

Admin có cookie xác thực; mọi form ghi dữ liệu có anti-forgery. Khách chỉ xem được đơn tạo trong session hiện tại hoặc Admin xem được mọi đơn. Session in-memory phù hợp demo một máy, bị mất khi restart; chưa có tài khoản khách và lịch sử mua hàng lâu dài.

## 4. Kết nối Compass

Dán URI:

```text
mongodb://localhost:27017/?replicaSet=rs0&directConnection=true
```

Chọn DB `ecommerce_dev`: `danhmuc`, `sanpham`, `hoadon`. Đọc `docs/COMPASS.md` để xem bộ lọc, import/export và kiểm tra kiểu BSON.

## 5. Chạy bộ kiểm chứng tự động

Giữ MongoDB chạy. Mở **terminal thứ hai** tại thư mục gốc:

```powershell
.\.venv\Scripts\python.exe tools/verify.py
```

Script build Release, chạy kiểm tra dữ liệu, tạo DB `_test` riêng, khởi động web riêng tại **5081**, chạy E2E qua HTTP + MongoDB thật, đo benchmark và kiểm tra roundtrip. Mật khẩu test được sinh ngẫu nhiên và chỉ truyền vào process test. DB thử được giữ lại với tên in ở cuối; DB demo không bị sửa. Kết quả được ghi vào `evidence/`.

`verify.py` cần lệnh `dotnet` có trong PATH. Script sẽ tạo dữ liệu benchmark 3.600 đơn (72 × 50) theo mặc định. Chạy `--scale 1` nếu chỉ cần 72 đơn. `tools/benchmark.py` có thể đo riêng; xem `docs/BENCHMARK.md`.

## 6. Cấu trúc và tài liệu

```text
src/Ecommerce.Web/  Model, interface, Mongo service, MVC Controller, Razor View, CSS
tools/             Tạo/seed dữ liệu, BSON import/export, benchmark, bộ verify
tests/             Unit test dữ liệu và E2E HTTP + DB thật
data/              Extended JSON canonical + báo cáo kỳ vọng từ phép tính độc lập
docs/              Contract, thiết kế, metric, Compass, kiểm thử, Git và demo
evidence/          Kết quả kiểm chứng được thực hiện trong quá trình bàn giao
.github/           CI và mẫu pull request
```

- `docs/HANDOFF.md`: đối chiếu từng task Jira với file bàn giao và việc còn chờ.
- `docs/CONTRACTS.md`: interface/route và quy ước để Tín/Tuấn tiếp quản.
- `docs/DATA_MODEL.md`, `docs/METRICS.md`: schema, snapshot và công thức.
- `docs/COMPASS.md`, `docs/BENCHMARK.md`, `docs/TESTING.md`: thao tác và kiểm chứng.
- `docs/DEMO.md`: lời dẫn và thứ tự bảo vệ cho cả ba người.
- `docs/SETUP_REPOSITORY.md`: tạo repo mới, mời thành viên, branch/PR/CI.

## 7. Dừng và mở lại

Dừng web bằng Ctrl+C. Dừng MongoDB:

```powershell
docker compose stop
```

Mở lại: `docker compose up -d --wait`, `tools/init_mongo.py`, rồi `dotnet run` như trên. Volume giữ dữ liệu. **Không dùng `docker compose down -v` nếu cần giữ DB.** Không chạy seed lại sau khi đã demo thay đổi dữ liệu.

## 8. Giới hạn bản bàn giao

Bản này phục vụ local và bảo vệ đồ án; chưa có thanh toán online, upload ảnh, tài khoản khách, triển khai production, lưu session phân tán hay phân quyền nhiều vai trò. Các hình khối trên catalog là minh họa CSS, không phải ảnh sản phẩm thật. Sản phẩm dùng thao tác **ngừng bán (soft delete)** để bảo toàn tham chiếu và khả năng hoàn kho.

Chưa tạo repo từ xa, chưa gửi lời mời thành viên, chưa có PR merge/review nhóm hay buổi diễn tập thật. Các bước đó cần tài khoản repo/nhóm và xác nhận thực tế; không được suy từ kiểm thử tự động thành đã hoàn tất Jira DoD.

## 9. Bản đã biên dịch và đóng gói

Xem `CHAY_NHANH.md` để chạy `publish/Ecommerce.Web.dll`. Để tạo lại bản đã biên dịch sau khi sửa source: `dotnet publish src/Ecommerce.Web -c Release --self-contained false -o publish`. Để đóng gói: `python tools/package.py --with-publish --out artifacts/Ecommerce_SV3_Full.zip`. Không cần đưa thư mục publish vào Git.
