# Dữ liệu mẫu

File canonical Extended JSON trong data/: 3 danh mục, 120 sản phẩm thuộc electronics/fashion/home, 72 hóa đơn với 2–5 items. Giá và tổng BSON Int64; thời gian UTC.
Nạp bằng database/seed.mongosh.js; xem ENVIRONMENT.md. ID và checkoutKey seed-v1 xác định dataset. Nạp lại dữ liệu giống nhau là no-op; DB có dữ liệu khác bị từ chối, không xóa/ghi đè.
Seed hiện dùng file có sẵn, không có generator hoặc tham số số lượng. Thay dataset cần PR cập nhật expected_report và test.
Test Node tùy chọn kiểm tra quota, tính duy nhất, snapshot/tổng và doanh thu kỳ vọng: node --test tests/data/*.test.mjs.
