using Ecommerce.Web.Models;
using Ecommerce.Web.Repositories;

namespace Ecommerce.Web.Services;

public sealed class MongoDashboardService : IDashboardService
{
    private readonly AnalyticsRepository analytics;

    public MongoDashboardService(AnalyticsRepository analytics)
    {
        this.analytics = analytics;
    }

    public async Task<DashboardView> Report(DateOnly from, DateOnly to, string group)
    {
        if (group != "day" && group != "month")
            throw new BusinessException("Chỉ hỗ trợ nhóm theo ngày hoặc tháng.");

        var (start, end) = ReportRange.Utc(from, to);
        var report = await analytics.GetReport(start, end, group);
        return new DashboardView(from, to, group, report.Revenue, report.Top);
    }
}
