# API Contract

Nguồn: contracts/openapi/ecommerce.v1.json (17 operations), chưa được cả nhóm xác nhận.
Implemented: GET categories/list+detail, products/list+detail, analytics/revenue+top-products+summary. Planned: tất cả API ghi và Orders. MVC cũ vẫn hoạt động riêng.
DTO public không expose checkoutKey/customerToken. ID là ObjectId hex 24 ký tự. Product page dùng q/categoryId/page, pageSize=12. Không hỗ trợ price sorting mới trong adapter hiện tại.
Errors dùng application/problem+json cho validation/not-found/auth trên API mới; lỗi ngoài dự kiến qua API exception handler. Mock fixtures trong contracts/examples/.
Giá VND nguyên, max 2^53-1 để không mất chính xác ở JS; Product input <=10^12. Trạng thái hiện có Pending→Completed/Cancelled; Completed là quy ước doanh thu hiện tại, không phải bằng chứng payment provider. Không thêm Paid trước migration.

Trước API ghi cần hoàn tất thiết kế CSRF token endpoint/header và owner session. Không bypass antiforgery toàn cục để chạy Next.js. GET đơn phải owner hoặc Admin; checkout không nhận giá; số lượng 1–99, tối đa 50 dòng; transaction và retry idempotent giữ theo service cũ.
