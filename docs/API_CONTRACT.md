# API Contract

Nguồn: contracts/openapi/ecommerce.v1.json (18 operations), chưa được cả nhóm xác nhận.
Implemented: CRUD categories và GET categories/csrf, GET products/list+detail, analytics/revenue+top-products+summary. Planned: API ghi Products và Orders. Xem CATEGORY_API.md để thử CRUD danh mục. MVC hiện tại vẫn hoạt động.
DTO public không expose checkoutKey/customerToken. ID là ObjectId hex 24 ký tự. Product page dùng q/categoryId/page, pageSize=12. Không hỗ trợ price sorting mới trong adapter hiện tại.
Errors dùng application/problem+json cho validation/not-found/auth trên API mới; lỗi ngoài dự kiến qua API exception handler. Mock fixtures trong contracts/examples/.
Giá VND nguyên, max 2^53-1 để không mất chính xác ở JS; Product input <=10^12. Trạng thái hiện có Pending→Completed/Cancelled; Completed là quy ước doanh thu hiện tại, không phải bằng chứng payment provider. Không thêm Paid trước migration.

Trước API ghi cần hoàn tất thiết kế CSRF token endpoint/header và owner session. Giữ antiforgery cho các thao tác ghi từ trình duyệt. GET đơn phải owner hoặc Admin; checkout không nhận giá; số lượng 1–99, tối đa 50 dòng; transaction và retry idempotent giữ theo service cũ.
