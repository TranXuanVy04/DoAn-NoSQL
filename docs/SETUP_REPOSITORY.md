# Thiết lập repository

Clone repository, mở Ecommerce.slnx. Toàn bộ source/data/docs được liệt kê trong Solution Explorer; file giữ nguyên vị trí trên ổ đĩa.
Đặt Mongo URI bằng User Secrets riêng mỗi máy; muốn dữ liệu chung thì cùng cluster/database và cấp tài khoản riêng. Một thành viên nạp seed ban đầu.
Kiểm tra bằng dotnet test và Node dataset tests tùy chọn. Chưa có CI workflow hoặc MongoDB integration job.
Thay alias CODEOWNERS bằng GitHub account thật, bật ruleset yêu cầu PR/review/checks, chặn force-push/delete main. Không coi việc có file CI là đã chạy trên remote.
