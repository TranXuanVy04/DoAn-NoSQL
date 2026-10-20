# Kiểm thử, bằng chứng và lỗi

## Tự động

`tools/verify.py` là một lệnh chạy đủ kiểm chứng. Có thể chạy riêng:

```powershell
.\.venv\Scripts\python.exe -m unittest discover -s tests -p "test_*.py" -v
```

Các test dữ liệu kiểm tra định mức, kiểu Int64/ObjectId/Date, JSON canonical khớp bộ sinh, snapshot độc lập, biên ngày UTC+7, trạng thái không được tính, thứ tự Top 5 và chặn tên DB ghi không phù hợp.

`tests/e2e.py` chạy qua HTTP + MongoDB thật trên `_test`: đăng nhập, chặn guest Admin, anti-forgery, phân trang/rỗng, tạo sản phẩm/validation/SKU trùng, danh mục còn hàng, checkout/snapshot/tồn kho, retry không trùng, quyền xem đơn, sửa giá sau mua, Completed, báo cáo đối chiếu độc lập theo ngày/tháng, khoảng rỗng/đảo, hủy lặp, cạnh tranh sản phẩm cuối và ngừng bán.

Chỉ chạy E2E trên DB thử cùng tên với web; script sẽ từ chối tên không kết thúc `_test`. Đây là test có ghi dữ liệu, không trỏ vào ecommerce_dev đang dùng bảo vệ. `verify.py` tự thiết lập môi trường đúng, mật khẩu test tạm và server 5081.

## Cần kiểm tra trực quan/thủ công trên máy nhóm

| Mã | Bước thực hiện | Kỳ vọng | Trạng thái bàn giao |
|---|---|---|---|
| M01 | Mở catalog desktop và điện thoại, dùng bàn phím Tab | Không tràn trang, nút/form đọc được | Chờ máy nhóm xác nhận |
| M02 | Compass xem electronics/fashion trong cùng sanpham | Thuộc tính khác nhau, BSON đúng | Chờ thao tác Compass |
| M03 | Compass export → import DB test mới | Counts và kiểu BSON đúng | Chờ thao tác Compass |
| M04 | Chạy README trên máy chưa có source/cache | Build và demo được | Chờ máy mới xác nhận |
| M05 | Cả ba người diễn tập 3 bước | Chuyển màn hình liên tục, giải thích đúng | Chờ nhóm |
| M06 | SV1/SV2 review PR, xác nhận contract | Không có đổi field ngầm, kiểm chứng trên bản chung | Chờ repo/nhóm |

Kết quả Python roundtrip là kiểm chứng script; không được ghi là đã thao tác Compass. Kiểm tra HTML qua HTTP không thay hoàn toàn kiểm tra render trong trình duyệt thật.

## Mẫu ghi bug để giao đúng module

```text
Tiêu đề: [Module] hành vi cụ thể
Jira/module: EC-... / SV1, SV2 hoặc SV3
Commit/nhánh:
Môi trường: .NET/Mongo/OS, tên DB thử
Tiền điều kiện và dữ liệu:
Bước tái hiện: 1. ... 2. ... 3. ...
Kỳ vọng:
Thực tế:
Mức độ: chặn demo / sai nghiệp vụ / giao diện
Bằng chứng: ảnh hoặc log đã loại mật khẩu, URI có credential
Kết quả kiểm tra lại sau sửa:
```

- Sai total/snapshot/kho/status → SV1.
- Sai revenue/top/timezone/index → SV2.
- Sai form/session/route/seed/benchmark → SV3 phối hợp module liên quan.

## Bằng chứng thật

Xem `evidence/VERIFICATION.md`, build log, unit log, e2e.json và benchmark.json trong gói bàn giao. Nếu chạy lại, các file máy mới thay file cũ; cần lưu riêng kết quả trước đó nếu muốn so sánh.

Không có kết quả tự động nào đồng nghĩa đã có người khác review hoặc đã merge PR.
