using Ecommerce.Web.Services;
using Ecommerce.Web.Models;
using Xunit;
public class ReportRangeTests {
    [Fact] public void LocalDayUsesUtcHalfOpenRange() {
        var (start, end) = ReportRange.Utc(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 1));
        Assert.Equal(new DateTime(2026, 7, 31, 17, 0, 0, DateTimeKind.Utc), start);
        Assert.Equal(TimeSpan.FromDays(1), end-start);
    }
    [Fact] public void ReversedRangeIsRejected() => Assert.Throws<BusinessException>(() => ReportRange.Utc(new DateOnly(2026, 8, 2), new DateOnly(2026, 8, 1)));
}
