# Bàn giao phần SV3 — Vỹ

Ngày lập: 19/10/2026. Stack theo Jira: ASP.NET Core MVC .NET10, Razor View, MongoDB. Chưa có repo/core chung nên bản bàn giao khởi tạo source và contract riêng để nhóm review.

## Đối chiếu 9 task

| Jira / mã kế hoạch | Sản phẩm bàn giao | Trạng thái thực tế |
|---|---|---|
| [EC-12 / KH-07](https://tantuan87264.atlassian.net/browse/EC-12) | Project MVC, layout, CSS, CI, PR template, CONTRIBUTING, README và hướng dẫn tạo remote | Mã đã có; remote/lời mời/milestone/check CI GitHub chờ tạo repo |
| [EC-13 / KH-08](https://tantuan87264.atlassian.net/browse/EC-13) | data/, generate_data.py, seed.py, validators.json, NO-OP/chặn ghi đè | Đã chạy thật: 120 sản phẩm / 3 ngành / 72 đơn, 2–5 dòng mỗi đơn |
| [EC-14 / KH-09](https://tantuan87264.atlassian.net/browse/EC-14) | Views Catalog, Admin Products/Categories, form validation, pagination và anti-forgery | Đã nối Mongo service và kiểm tra HTTP; tiếp quản core cần SV1 review |
| [EC-21 / KH-16](https://tantuan87264.atlassian.net/browse/EC-21) | Cart session, thêm/bỏ/đổi số lượng, Checkout View, tổng server, idempotency | Đã kiểm tra với DB thật; có transaction/rollback/chống bán vượt tồn |
| [EC-22 / KH-17](https://tantuan87264.atlassian.net/browse/EC-22) | Order detail snapshot, list/status, Dashboard ngày/tháng, biểu đồ, Top 5 | Đã nối Aggregation thật, đối chiếu HTML/DB/phép tính độc lập; chờ SV2 duyệt metric |
| [EC-23 / KH-18](https://tantuan87264.atlassian.net/browse/EC-23) | tests/, verify.py, evidence/, TESTING.md và mẫu bug | Có kết quả tự động thật; còn kiểm tra trên bản nhóm và thao tác trực quan |
| [EC-30 / KH-25](https://tantuan87264.atlassian.net/browse/EC-30) | benchmark.py, transfer_data.py, BENCHMARK.md, COMPASS.md | Đã đo/roundtrip qua script; Compass UI và gửi số đo cho SV2 chờ thực hiện |
| [EC-31 / KH-26](https://tantuan87264.atlassian.net/browse/EC-31) | Source, bản publish đã biên dịch, README máy mới, hướng dẫn và bằng chứng | Build/publish/smoke đạt trên môi trường kiểm chứng; cần chạy lại trên máy nhóm |
| [EC-32 / KH-27](https://tantuan87264.atlassian.net/browse/EC-32) | DEMO.md: phân vai 3 bước, lời dẫn, dự phòng, checklist bàn giao | Kịch bản đã chuẩn bị; chưa có buổi diễn tập/chấp thuận nhóm/tag release |

Các Epic EC-4/EC-5 là phạm vi tổng hợp. EC-6 thuộc Tín, không tính là task riêng của Vỹ.

## Phần core hỗ trợ chạy bản khởi tạo

Không chỉ bàn giao các View trống: `MongoCatalogService`, `MongoOrderService`, `MongoDashboardService` là implementation chạy thật và được kiểm chứng. Tuy vậy, chúng chạm phạm vi SV1/SV2; bàn giao ownership để các bạn review, phát triển và thay thế qua interface. Không coi việc sinh core nền là đã hoàn thành toàn bộ nhiệm vụ cá nhân của Tín/Tuấn.

## Chưa đáp ứng Definition of Done Jira ở cấp nhóm

Chưa có remote repo, người review khác, PR merge vào main, người nhận module xác nhận trên bản chung, thao tác Compass thủ công và diễn tập cả nhóm. Chưa tạo tag v1.0.0. Không tự chuyển Done hay đăng bình luận lên Jira trong lần bàn giao này.

## Trình tự tiếp nhận

1. Vỹ chạy README trên Windows; chạy verify.py và giữ log máy mình.
2. Tạo repo, mời đúng GitHub username của Tín/Tuấn theo SETUP_REPOSITORY.
3. Cả nhóm review CONTRACTS, DATA_MODEL, METRICS và giới hạn đã ghi.
4. Tín/Tuấn tiếp quản service; ghép qua interface, không tự đổi field không báo nhau.
5. Vỹ thao tác Compass, đo lại benchmark, kiểm tra máy mới và diễn tập.
6. Link PR/bằng chứng vào Jira; sau review/merge/xác nhận mới chốt Done và release.
