# Workflow ba thành viên

| Người | Phạm vi | Cách làm độc lập |
|---|---|---|
| SV1 | Models, validation, Categories/Products/Orders services và API | Interface, fake và JSON fixtures |
| SV2 | Analytics, pipeline, index/explain | Seed và expected_report, fake dashboard |
| SV3 | Razor Views, UX, JavaScript, dataset, E2E và demo | ViewModel/interface và fake services |

Controller/View cùng gọi interface service; không chờ Mongo implementation hoàn thành mới làm Razor. Fake chỉ đăng ký trong môi trường test.
Chốt API contract/model chung trước breaking change. Program.cs, Models, Contracts và validator cần review của người bị ảnh hưởng.
main → feature/<feature> → PR → review → merge; Conventional Commits. Không force-push branch người khác. Trưởng nhóm bật branch protection và required checks khi có CI tương ứng; repo hiện chưa có workflow CI.
