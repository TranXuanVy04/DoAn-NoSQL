# Báo cáo kiểm thử 22/10/2026

Kiến trúc hiện tại: ASP.NET Core MVC/Razor + Web API + MongoDB.
Kiểm tra sau đợt dọn: build và xUnit backend; Node dataset/contract tests đã chuyển sang tests/data/; kiểm tra solution references và diff whitespace.
Kết quả sau khi gỡ giao diện riêng: build backend và 8 xUnit tests PASS; 7 Node dataset/contract tests PASS; solution references và git diff --check PASS.
Chưa kiểm tra browser E2E, transaction/concurrency và benchmark trên Atlas thật. API ghi còn planned; MVC đã có luồng nghiệp vụ nhưng cần nhóm kiểm chứng tích hợp.

## CRUD danh mục

Build + 18 xUnit tests PASS (10 test mới cho CRUD, validation, ID, Admin, CSRF và conflict; 8 test có sẵn). HTTP tests dùng fake, không ghi Atlas.

## Tách Repository

Có 4 Repository cụ thể, đăng ký DI bằng class, không thêm interface Repository. Service không còn Find/Insert/Update/Delete/Aggregate trực tiếp. Các truy vấn của checkout/cancel vẫn nhận cùng session và cancellation token từ transaction ở Service. Build và 18 xUnit tests PASS; tests HTTP hiện dùng fake Service, chưa chứng minh truy vấn Repository/transaction trên Atlas thật.
