# Kiểm tra truy vấn

Đặt MONGO_DB thành database development/test phù hợp rồi chạy từ root:

mongosh "$env:MONGO_URI" --file database/explain.mongosh.js

Script chỉ đọc executionStats của catalog và pipeline top 5. Tạo indexes riêng bằng database/indexes.mongosh.js nếu tài khoản có quyền.
Chưa có benchmark runner tự động hoặc báo cáo hiệu năng mới. Khi đo, dùng DB _bench riêng, ghi server version, số document, index, query, warmup, nhiều lượt, median/p95, docs/keys examined và execution plan. 72 đơn quá nhỏ để kết luận lợi ích index.
Không drop index trên DB dùng chung để đo; chỉ dùng bản sao benchmark độc lập.
