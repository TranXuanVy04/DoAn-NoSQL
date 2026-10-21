# MongoDB Compass

Kết nối bằng URI Atlas/server được nhóm cấp. Chọn ecommerce_dev để xem danhmuc, sanpham và hoadon.
Không đưa URI có mật khẩu vào ảnh demo hoặc Git.

Nạp seed đầy đủ validator/index/transaction bằng database/seed.mongosh.js theo ENVIRONMENT.md.
Để thực hành import thủ công: tạo database mới có hậu tố _test, dùng Add Data → Import JSON với data/danhmuc.json, data/sanpham.json và data/hoadon.json. Các file dùng canonical Extended JSON để giữ ObjectId/Int64/Date. Import thủ công không tự tạo validator/index.

Export JSON với Extended JSON khi công cụ hỗ trợ, rồi kiểm tra lại kiểu BSON, số lượng 3/120/72 và tổng hóa đơn. Chưa có kiểm thử roundtrip tự động cho thao tác Compass hiện tại; ghi lại bằng chứng sau khi thực hiện thật.
