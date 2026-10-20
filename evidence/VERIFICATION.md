# Kết quả kiểm chứng thực tế

Ngày: 19/10/2026. Môi trường: Linux, .NET SDK 10.0.402 / Runtime 10.0.13, MongoDB 8.0.17, MongoDB.Driver 3.12.0, Python 3.12.14. Đây không phải kết quả trên máy Windows của nhóm.

| Hạng mục | Kết quả |
|---|---|
| Build Release | Thành công, 0 lỗi, 0 cảnh báo |
| Unit/data | 7/7 đạt |
| E2E HTTP + DB thật | 34/34 đạt |
| Seed / chạy lại | 3 danh mục, 120 sản phẩm, 72 đơn; chạy lại NO-OP |
| Snapshot | Sửa giá/thuộc tính không đổi đơn cũ |
| Tồn kho | Cạnh tranh sản phẩm cuối không bán vượt tồn; lỗi một dòng rollback toàn bộ |
| Chống submit trùng | Một checkoutKey chỉ tạo một đơn và trừ kho một lần |
| Dashboard | HTML và Aggregation khớp phép tính độc lập theo ngày/tháng |
| Roundtrip script | Toàn bộ 3/120/72 documents và BSON giữ nguyên |
| Publish | DLL đã khởi động và trả MVC catalog + CSS thành công |
| Trình duyệt / Compass UI / Windows mới | Chưa xác nhận; xem TESTING.md |

## Benchmark đã đo

3600 đơn; scale=50; warm-up=5, số lượt=30; lọc 2026-08-01 đến hết 2026-10-31 UTC+7. Đo client đọc hết cursor.

| Pipeline | Median trước (ms) | p95 trước (ms) | Median sau (ms) | p95 sau (ms) |
|---|---:|---:|---:|---:|
| Doanh thu | 7.494 | 15.766 | 9.195 | 14.187 |
| Top 5 | 18.713 | 25.918 | 19.464 | 23.553 |

Explain chuyển từ COLLSCAN sang IXSCAN, nhưng median lần đo này tăng sau index. Khoảng lọc rộng và dữ liệu nhỏ nên không kết luận index luôn nhanh hơn. Kết quả trước/sau giống nhau; nguồn không bị thay đổi. Full samples/explain ở benchmark.json.

## Phạm vi bằng chứng

Kiểm thử chạy thật qua Mongo replica set local và MVC; không dùng mock để thay báo cáo doanh thu. Chưa chạy GitHub Actions trên remote vì chưa có repo. Đã thử mở trình duyệt tự động nhưng môi trường chặn Unix socket cần cho Chrome, nên không xác nhận kiểm tra render desktop/mobile. Không lưu ảnh chụp trình duyệt vào gói bàn giao. Không suy kết quả HTTP thành đã thao tác Compass hoặc review nhóm.
