# Báo cáo kiểm thử 22/10/2026

Kiến trúc hiện tại: ASP.NET Core MVC/Razor + Web API + MongoDB.
Kiểm tra sau đợt dọn: build và xUnit backend; Node dataset/contract tests đã chuyển sang tests/data/; kiểm tra solution references và diff whitespace.
Kết quả sau khi gỡ giao diện riêng: build backend và 8 xUnit tests PASS; 7 Node dataset/contract tests PASS; solution references và git diff --check PASS.
Chưa kiểm tra browser E2E, transaction/concurrency và benchmark trên Atlas thật. API ghi còn planned; MVC đã có luồng nghiệp vụ nhưng cần nhóm kiểm chứng tích hợp.

## CRUD danh mục

Build + 18 xUnit tests PASS (10 test mới cho CRUD, validation, ID, Admin, CSRF và conflict; 8 test có sẵn). HTTP tests dùng fake, không ghi Atlas.

## Tách Repository

Có 4 Repository cụ thể, đăng ký DI bằng class, không thêm interface Repository. Service không còn Find/Insert/Update/Delete/Aggregate trực tiếp. Các truy vấn của checkout/cancel vẫn nhận cùng session và cancellation token từ transaction ở Service. Build và 18 xUnit tests PASS; tests HTTP hiện dùng fake Service, chưa chứng minh truy vấn Repository/transaction trên Atlas thật.

## ParentId thay Category.Kind

Schema/request/form/API/seed và OpenAPI v0.2.0 đã đổi sang ParentId. Xác thực cha tồn tại, self/descendant/cycle; không xóa danh mục còn con. BSON roundtrip và đọc dữ liệu legacy có test. Script migration có dry run và backup, chưa chạy trên Atlas. Build và 29 xUnit tests, 8 Node tests PASS.

## Giao diện Ogani

Tích hợp assets từ ogani-v1.0.0 vào Razor cho trang chủ/shop/details/cart/checkout/blog/contact; giữ Admin/Account layout riêng. Build Razor không lỗi/cảnh báo; 41 xUnit tests PASS, gồm 12 HTTP tests giao diện, session cart→checkout/antiforgery và chặn demo vào giỏ. 8 Node dataset tests PASS.
Chrome headless kiểm tra home/shop/detail desktop 1440px và home/menu mobile 390px: không tràn ngang, không ảnh lỗi, không JavaScript exception; yêu thích local và mở/đóng menu bằng Escape hoạt động. Ảnh ở artifacts/ogani-preview (ignored). Tất cả kiểm tra dùng dữ liệu fake/demo local, không thay đổi Atlas; chưa xác nhận toàn bộ checkout/transaction với DB thật. Xem OGANI_TEMPLATE.md cho danh sách mock và cách thay nội dung.

## Hoàn thiện CRUD sản phẩm có biến thể

Form /Admin/ProductEdit hỗ trợ tạo/sửa biến thể, ngừng bán theo biến thể và hiển thị giá từ/tổng tồn tại /Admin/Products. Kiểm tra SKU, tổ hợp lựa chọn, kiểu thuộc tính danh mục gốc và revision chống ghi đè tồn kho. Giữ ID biến thể đã lưu để hoàn kho đơn cũ.

Kiểm tra cập nhật: 42 kiểm thử .NET đạt (gồm form MVC thêm/sửa/ngừng bán, stale form, hai biến thể cùng giỏ, xóa riêng từng biến thể, snapshot giá/options); 6 kiểm thử dữ liệu đạt. Chrome kiểm tra desktop 1440px và mobile 390px: thêm/xóa dòng biến thể, chuyển chế độ và không tràn ngang/không lỗi JavaScript. Không chạy ghi đơn hoặc transaction trên Atlas thật trong lượt này. Xem PRODUCT_VARIANTS.md để demo trên Atlas và chuẩn bị validator/index.
