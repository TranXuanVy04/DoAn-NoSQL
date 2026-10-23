# Task board

| ID | Owner | Module / Branch | Contract / Dependency / Mock | Acceptance criteria / Test | Status |
|---|---|---|---|---|---|
| F00 | Cả nhóm | Shared / feature/contract-review | Model, interface, OpenAPI; JSON examples | Chốt field/trạng thái và review thay đổi chung | Ready |
| B01 | SV1 | Catalog / feature/catalog-api | ICatalogService; fake catalog | Hoàn thiện API ghi nếu cần; auth/CSRF, validation/duplicate errors | MVC có sẵn; API ghi planned |
| B02 | SV1 | Orders / feature/order-api | IOrderService; order fixtures | Kiểm tra transaction, retry, snapshot và quyền xem trên Atlas test | Service/MVC có sẵn; DB test pending |
| B03 | SV1 | Schema / feature/schema-migration | Domain/validator/seed | Chốt field còn thiếu theo master prompt, migration có review | Todo |
| A01 | SV2 | Analytics / feature/analytics-api | IDashboardService; expected_report/fake dashboard | So khớp doanh thu Completed, ngày/tháng và Top 5 với DB thật | API/MVC có sẵn; verify pending |
| A02 | SV2 | Index / feature/mongodb-indexing | database scripts + seed | executionStats, nhiều lượt đo trên DB riêng, không khẳng định tăng tốc thiếu số liệu | Script có sẵn; benchmark pending |
| W01 | SV3 | Razor catalog / feature/razor-catalog | ProductPage, ICatalogService/fake | Kiểm tra danh sách, chi tiết, phân trang, empty/error và mobile | Views có sẵn; browser review pending |
| W02 | SV3 | Razor checkout / feature/razor-checkout | Cart/Checkout ViewModel, IOrderService/fake | UX giỏ/checkout, server tính giá, snapshot/retry/ownership | Views có sẵn; E2E pending |
| W03 | SV3 | Razor Admin / feature/razor-admin | Forms/DashboardView, fake dashboard | CRUD/Admin login/antiforgery, biểu đồ và filters | Views có sẵn; browser review pending |
| Q01 | Cả nhóm | QA / test/integration | Database _test riêng; fake HTTP tests | CRUD, concurrency, transaction rollback và dashboard với Atlas test | In progress |
| D01 | SV3 | Demo / docs/demo | Q01, Compass, dataset | 4 kịch bản bảo vệ, bằng chứng mới và rehearsal | Todo |
