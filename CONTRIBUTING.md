# Quy ước cộng tác

Đọc `docs/CONTRACTS.md` và `docs/SETUP_REPOSITORY.md` trước sửa. Mỗi người một nhánh, PR vào main, thành viên khác review. Tín phụ trách catalog/đơn; Tuấn phụ trách Aggregation/index; Vỹ phụ trách giao diện/tích hợp, dummy data, đo/kiểm chứng/demo.

Thay schema/contract/metric phải cập nhật source, validator, seed, fixture, test và docs trong cùng PR. Chạy `dotnet build` và `tools/verify.py` với DB thử trước đề nghị merge. Không commit bí mật, môi trường ảo, bin/obj hoặc dữ liệu người dùng thật.

Tiền VND Int64; UTC trong DB; UTC+7 cho ngày báo cáo. Không chuyển đơn sang lưu chỉ productId và lookup giá hiện tại. Không drop DB/index dùng chung để benchmark; dùng bản sao _bench mới. Không ghi kết quả đo chưa thực hiện hoặc nhận xét review chưa có người xác nhận.
