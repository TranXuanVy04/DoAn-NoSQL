# Báo cáo kiểm thử 21/10/2026

Stack kiểm tra hiện tại: .NET SDK 10.0.401, Node.js 24.12.0/npm 11.6.2.
Kết quả sau đợt dọn công cụ:

| Kiểm tra | Kết quả |
|---|---|
| dotnet test Ecommerce.slnx -c Release --no-restore | PASS: build + 8 xUnit tests |
| Frontend typecheck / lint | PASS |
| npm test | PASS: 7 Node tests (2 contract + 5 dataset/snapshot/revenue) |
| Next.js production build | PASS |
| node --check database/*.mongosh.js (từng file) | PASS: cú pháp 3 script |
| Solution file references / git diff --check | PASS |

Lần chạy .NET trong sandbox gặp giới hạn named pipe; lần chạy ngoài sandbox đã pass.
Database scripts mới dùng mongosh; chưa chạy seed/index/explain trên server thật vì chưa có kết nối DB nhóm.
HTTP xUnit dùng fake; không chứng minh transaction/stock concurrency trên MongoDB thật. Chưa có browser E2E cho Next.js.

Bằng chứng trong evidence/ và manifest CONTENTS.sha256 là lịch sử trước đợt dọn, có thể nhắc công cụ cũ, không đại diện cho repo hoặc kết quả hiện tại. Không xóa các bằng chứng lịch sử này.

API ghi/Orders, Next.js checkout/Admin, migration field và Mongo integration còn nằm trong TASK_BOARD.md. Chuỗi kết nối mặc định rỗng; cấu hình User Secrets/environment theo ENVIRONMENT.md trước khi chạy backend thật.
