# Task board

| ID | Task | Owner | Module | Branch | Contract | Dependency | Mock | Acceptance criteria | Test checklist | Status |
|---|---|---|---|---|---|---|---|---|---|---|
| F00 | Duyệt contract và mapping BSON | Cả nhóm | Shared | feature/contract-review | all | schema vocabulary | examples | Thống nhất Completed và mapping fields; chốt CSRF | contract validator + review | Ready |
| B01 | API ghi Categories/Products | SV1 | Catalog | feature/catalog-api | Categories/Products | ICatalogService | fake catalog | CRUD có auth/CSRF, conflict và validation | API integration 400/401/403/409 | Todo |
| B02 | API checkout và quyền xem đơn | SV1 | Orders | feature/order-api | Orders | IOrderService | sample order | Snapshot server; owner session; retry idempotent; replica set transaction | snapshot + race stock + rollback + ownership | Todo |
| B03 | Bổ sung field theo master prompt | SV1 | Schema | feature/schema-migration | Product/Order | legacy dataset | 3 polymorphic examples | Migration explicit; validator/seed/BSON đồng bộ | roundtrip serialization + rollback migration | Todo |
| A01 | Analytics API và summary | SV2 | Analytics | feature/analytics-api | Analytics | IDashboardService | fake dashboard + seed | GET có auth; timezone + doanh thu Completed | so sánh pipeline với expected_report | Adapter ready; DB verify pending |
| A02 | Index và explain | SV2 | Analytics | feature/mongodb-indexing | Analytics | seed | database/explain.mongosh.js | Chọn index theo query thật, ghi executionStats | nhiều lượt đo + dataset lớn riêng | Mongosh script ready; DB verify pending |
| W01 | Next.js catalog | SV3 | Catalog UI | feature/frontend-catalog | Products | API GET | mockProducts | UI gọi service, mock/real bằng env | typecheck + lint + build | Skeleton ready |
| W02 | Cart/checkout/chi tiết | SV3 | Orders UI | feature/frontend-cart | Orders | B02 | order fixtures | localStorage không chứa giá đáng tin; checkout server tính lại | E2E order + sửa giá gốc | Todo |
| W03 | Admin CRUD và Dashboard | SV3 | Admin UI | feature/admin-dashboard | Analytics/CRUD | B01/A01 | analytics fixtures | login/CSRF; biểu đồ, filters và top 5 | E2E auth + dashboard | Todo |
| Q01 | Contract compatibility và test DB | Cả nhóm | QA | test/contract-integration | all | B01/B02/A01 | fake services | Mock và thật cùng schema; DB test riêng | CI green + fixture schema validation | In progress |
| D01 | Demo và báo cáo cuối | SV3 | Demo | docs/demo | all | Q01 | MVC fallback | 4 kịch bản có evidence mới, không dùng số liệu cũ | Compass + benchmark + snapshot | Todo |
