# FEATURE-ID — Tên feature
Owner: SV1/SV2/SV3. Branch: feature/name. Status: Todo.

## Trigger và hành vi
Ví dụ: khách đổi số lượng giỏ từ 1 thành 2; checkout đọc lại giá DB và trả snapshot đã lưu.

## Contract và dependency
Operation: POST /api/orders. Fake: contracts/examples/order.json. Không nhận giá từ client.

## Acceptance criteria
- Hàng inactive/thiếu kho bị từ chối.
- Giá đơn cũ giữ nguyên khi catalog đổi.
- Retry không tạo thêm đơn.

## Test report
Lệnh, môi trường, số lượng test, kết quả thực chạy và giới hạn; đính kèm evidence mới.

## Demo và rollback
Demo snapshot; rollback code không xóa hóa đơn hoặc tự reset database.
