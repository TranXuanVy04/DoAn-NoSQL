# AGGREGATION

Pipeline thật: Repositories/AnalyticsRepository.cs; MongoDashboardService kiểm tra khoảng ngày và gọi repository; query plan tham khảo database/explain.mongosh.js. Có $match/$unwind/$group/$sort, không $lookup snapshot. Chỉ Completed; UTC lưu trữ, UTC+7 nhóm ngày. Không tính Pending/Cancelled.

Xem tài liệu chi tiết hiện có: [METRICS.md](METRICS.md).
