# Demo

1. SV3 mở Compass, chọn sanpham và so sánh attributes giữa ba ngành.
2. SV1 dùng MVC thêm giỏ/checkout, kiểm tra hoadon.items; đổi giá sản phẩm gốc và xác nhận đơn cũ giữ snapshot.
3. SV2 mở Admin Dashboard, lọc 2026-08-01 đến 2026-10-31 và so sánh doanh thu Completed với data/expected_report.json.
4. Chạy database/explain.mongosh.js, giải thích compound index và executionStats. Chưa có benchmark định lượng mới.

Trước demo: kiểm tra kết nối Atlas/server, quyền DB, mật khẩu Admin, seed và build. Next.js mới có catalog; dùng MVC cho luồng đầy đủ. Không reset dữ liệu nhóm trong lúc demo.
