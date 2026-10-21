# Workflow cho ba sinh viên

Phase 0: cả nhóm review contract, tiền tệ, trạng thái, schema và quyền truy cập trước implementation ghi. Contract đã có GET thực tế, các operation planned không được coi đã chạy.

| Người | Sở hữu | Dependency thay thế |
|---|---|---|
| SV1 | Categories/Products/Orders + BSON | xUnit fake và JSON examples; không chờ Next.js |
| SV2 | Analytics, index và explain | 72 invoices seed + expected_report; không chờ Orders API |
| SV3 | Next.js, mocks, seed, E2E | mockProducts + fixture cho module mới; không chờ API |

Phase 1 chạy đồng thời theo task board. Phase 2 thay env mock=false, kiểm tra contract/CSRF/session/checkout/snapshot. Phase 3 build, DB integration, benchmark và rehearsal.

`main` → `feature/<feature>` → PR → review → squash merge; Conventional Commits. Không push main hoặc force-push branch người khác.
Shared files Program.cs, Models/Domain.cs, Services/Contracts.cs, contracts/, database/validators.json cần trưởng nhóm và người bị ảnh hưởng review.
CODEOWNERS hiện có alias mẫu trong comment để tránh gán sai GitHub account. Trưởng nhóm điền account thật và bật ruleset: PR bắt buộc, >=1 approval, dismiss stale approvals, require code-owner review, block force-push/delete, checks backend/frontend/integration phải pass, resolve conversations.
Không coi file CI là đã bật branch protection trên GitHub; thao tác remote này chưa thực hiện.
