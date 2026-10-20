# Kịch bản bảo vệ và lời dẫn — Vỹ điều phối

## Chuẩn bị 15 phút trước demo

- MongoDB rs0 đang primary, web 5080 đang chạy, Admin đã đăng nhập.
- Dùng ecommerce_dev có seed và không trộn DB benchmark/test.
- Mở sẵn 3 tab: catalog, Admin Products/Orders, Dashboard với `from=2026-08-01&to=2026-10-31`.
- Compass mở ecommerce_dev, cả sanpham và hoadon; chuẩn bị filter trong phần dưới.
- Kiểm tra counts; nếu đã đặt thêm đơn thì nói số hiện tại, không đọc máy móc 72.
- Chọn hai sản phẩm active còn kho; ghi SKU và giá trước mua; không dùng sản phẩm đã ngừng bán.
- Xuất DB dự phòng ra thư mục mới trước diễn tập, không drop/reset giữa buổi.

## Bước 1 — Vỹ: cùng collection, thuộc tính khác nhau (1–2 phút)

Mở Compass sanpham. Filter electronics rồi fashion; mở attributes.

Lời dẫn: “Collection sản phẩm có các trường chung như tên, giá, tồn kho. Trong cùng collection, điện tử có brand và warrantyMonths; thời trang có size và material. Trường kind phân biệt ngành, attributes lưu phần thuộc tính thay đổi. JSON Schema kiểm soát thuộc tính bắt buộc của từng ngành.”

Cho xem số sản phẩm theo ngành và định mức. Chuyển cho Tín.

## Bước 2 — Tín đặt hàng, Vỹ chứng minh snapshot (2–3 phút)

1. Tín trên web thêm hai mặt hàng, đổi số lượng, mở giỏ, điền thông tin giả và đặt đơn.
2. Vỹ ghi mã đơn trang kết quả, filter hoadon bằng number.
3. Vỹ chỉ items có name, attributes, unitPrice, quantity, lineTotal; total đúng tổng dòng, không chỉ lưu productId.
4. Tín sửa giá/tên/thuộc tính một sản phẩm trên Admin. Tải form lại trước sửa để có tồn kho mới.
5. Vỹ refresh đơn cũ và chỉ giá/thuộc tính lịch sử không đổi. Mở chi tiết đơn trên web cũng đọc snapshot.

Lời dẫn: “Giá server được đọc lại lúc tạo đơn. Giá và chi tiết được nhúng vào mảng items nên đơn cũ giữ đúng giao dịch, không phụ thuộc dữ liệu sản phẩm đã thay đổi. Tạo đơn và trừ kho cùng transaction.”

Nếu bấm lại submit, cùng checkoutKey chỉ trả lại đơn cũ. Không phải checkout mới luôn bị chặn.

## Bước 3 — Tuấn: Dashboard động (2–3 phút)

1. Dashboard ban đầu không tính đơn Pending vừa tạo.
2. Admin mở đơn vừa tạo và bấm Hoàn tất; Dashboard refresh với khoảng lọc **có ngày hôm nay**.
3. Doanh thu tăng đúng **total của đơn**, không dùng giá sản phẩm đã sửa. Top 5 có thể đổi thứ tự nếu số lượng đủ lớn; không hứa mọi đơn nhỏ đều làm Top 5 thay đổi.
4. Tuấn giải thích `$match`, `$group`, `$sort` của revenue; `$unwind` và nhóm snapshot ở Top 5; không `$lookup`.
5. Vỹ mở file benchmark đã đo; Tuấn giải thích index và giới hạn kết luận.

Đơn được xếp kỳ theo **createdAt UTC+7**, chỉ Completed. Khi demo vào ngày ngoài tháng 8–10/2026, phải sửa bộ lọc để bao gồm ngày thực hiện; seed cũ vẫn xem được bằng khoảng riêng.

## Phương án dự phòng

| Tình huống | Xử lý |
|---|---|
| Mongo chưa sẵn sàng | Kiểm tra Docker Desktop, `docker compose ps`, chạy init_mongo.py |
| Checkout báo standalone | Dùng đúng URI rs0, không nối nhầm Mongo local standalone |
| Cổng 27017/5080 bận | Dừng dịch vụ trùng; nếu đổi cổng phải đổi mọi cấu hình liên quan |
| Đơn không hiện Dashboard | Kiểm tra Completed, khoảng createdAt và UTC+7 |
| Sửa sản phẩm bị từ chối tồn kho | Tải lại form rồi sửa, không dùng form trước checkout |
| Mất session/restart web | Admin vẫn có thể mở đơn qua danh sách; khách cũ không tự xem lại được sau session mất |
| Compass import sai kiểu | Dùng DB test mới, file canonical, kiểm tra BSON; không sửa DB demo trực tiếp |
| Mất mạng ngoài | Sau restore/build/image pull thành công, demo local không cần internet |

Nếu web gặp lỗi, dùng dữ liệu/ảnh bằng chứng đã có để giải thích rồi khôi phục; nói rõ phần dự phòng, không trình bày ảnh cũ như giao dịch đang chạy.

## Bảng bàn giao cuối

Vỹ ghi ngày/commit, cấu hình, kết quả build/test, file benchmark, export dự phòng; Tín xác nhận CRUD/đơn; Tuấn xác nhận metric/index; người review ghi PR. **Chỉ sau cả nhóm xác nhận và kiểm tra bản chung mới tạo tag v1.0.0.** Chưa có xác nhận thì dùng bản bàn giao để review, không tự gọi release chính thức.
