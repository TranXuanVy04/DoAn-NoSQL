# Đóng góp

main → feature/<feature> → Pull Request → review → merge. Dùng Conventional Commits; không push trực tiếp main hoặc force-push branch người khác.
Contract/schema/shared config phải cập nhật model, validator, fixture, test và docs cùng PR, có review của module bị ảnh hưởng.
Chạy dotnet test Ecommerce.slnx -c Release và các lệnh typecheck/lint/test/build tại src/storefront.
Không commit secrets, node_modules, bin/obj hoặc dữ liệu khách hàng thật.
Xem docs/TEAM_WORKFLOW.md và docs/TASK_BOARD.md.
