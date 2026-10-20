# Benchmark Aggregation trước/sau compound index

## Chạy

Giữ MongoDB chạy, DB nguồn đã seed. Tại thư mục gốc:

```powershell
.\.venv\Scripts\python.exe tools/benchmark.py --source ecommerce_dev --target vy_run01_bench --scale 50 --warmup 5 --runs 30 --from 2026-08-01 --to 2026-10-31 --out evidence/benchmark-run01.json
```

Đích phải là **DB mới/trống khác nguồn**, có đuôi `_bench`. Chạy lại chọn `vy_run02_bench`, không drop index nguồn. Script chỉ đọc hoadon nguồn, chép thành collection thử, không sửa trạng thái/giá/đơn nguồn.

`--scale 1` = 72 đơn khi nguồn seed nguyên vẹn; `--scale 50` = 3.600; `--scale 1000` = 72.000. Lặp order có ID/checkoutKey/number mới; item snapshot giữ nguyên. Đây là dữ liệu nhân bản cho workload, không đại diện đầy đủ phân phối thực tế. Không dùng bộ nhân bản thay 120 sản phẩm/72 đơn trong demo chính.

## Phương pháp

1. Ghi phiên bản server/Python, nền tảng, số document, khoảng lọc và thời điểm.
2. Đo trước index, chỉ có `_id` index. Mỗi pipeline warm-up 5 lượt riêng.
3. 30 lượt đo bằng `perf_counter_ns`, tính từ gọi aggregate đến khi đọc hết cursor thành list.
4. Tạo index `{status:1,createdAt:1}` trên **bản sao**.
5. Warm-up lại và đo cùng pipeline/khoảng ngày/số lượt/dataset.
6. So sánh nguyên kết quả trước/sau; xuất full explain `executionStats` cho cả hai phase.
7. Kiểm tra document nguồn vẫn giữ nguyên tại cuối phép đo. Nếu có người mua/sửa đơn nguồn cùng lúc thì báo lỗi, cần đo lại khi không có ghi.

Median = trung vị mẫu thời gian client. p95 = nearest rank, phần tử `ceil(0.95*n)` trong dãy tăng. Không lấy `executionTimeMillis` làm median/p95; thông số đó được lưu riêng trong explain.

## Cách đọc báo cáo

`phases.before/after.revenue/top5` chứa medianMs, p95Ms, samplesMs, explain và resultRows. Tìm `COLLSCAN`/`IXSCAN`, totalDocsExamined, totalKeysExamined trong explain (có thể nằm trong stages hoặc queryPlanner theo phiên bản).

Ghi cho SV2:

| Nội dung | Cần báo |
|---|---|
| Môi trường | CPU/RAM/OS, MongoDB, Python, có workload khác hay không |
| Dữ liệu | N đơn, 2–5 dòng, scale, phân bố ngày/status |
| Truy vấn | Pipeline, khoảng lọc, timezone và index |
| Kết quả | Median/p95 trước/sau; docs/keys examined; kết quả có giữ nguyên không |
| Giới hạn | Cache đã warm, chạy before rồi after, tập nhỏ hoặc filter ít chọn lọc |

Index có thể không nhanh hơn với 72/3.600 document và khoảng ngày rất rộng. Không sửa số đo để tạo kết luận tốt hơn. Muốn đo workload chọn lọc hơn, chạy **một benchmark khác** với khoảng hẹp, giữ cùng khoảng cho before/after; không so trước khoảng rộng với sau khoảng hẹp.

`evidence/benchmark.json` là kết quả đã đo lúc bàn giao, chỉ đại diện môi trường đó. Vỹ cần đo lại trên máy bảo vệ và gửi file thực tế cho Tuấn. Việc gửi file/giao W3 D1 chưa được thực hiện tự động.
