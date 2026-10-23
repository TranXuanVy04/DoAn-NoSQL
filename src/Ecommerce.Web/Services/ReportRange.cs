using Ecommerce.Web.Models;

namespace Ecommerce.Web.Services;

public static class ReportRange {
    public static (DateTime Start, DateTime End) Utc(DateOnly from, DateOnly to) {
        if (to < from || to == DateOnly.MaxValue || to.DayNumber-from.DayNumber > 3660) throw new BusinessException("Khoảng ngày không hợp lệ (tối đa 10 năm, ngày kết thúc trước 31/12/9999).");
        return (DateTime.SpecifyKind(from.ToDateTime(TimeOnly.MinValue).AddHours(-7), DateTimeKind.Utc),
            DateTime.SpecifyKind(to.AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(-7), DateTimeKind.Utc));
    }
}
