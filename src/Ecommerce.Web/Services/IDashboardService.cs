using Ecommerce.Web.Models;

namespace Ecommerce.Web.Services;

public interface IDashboardService {
    Task<DashboardView> Report(DateOnly from, DateOnly to, string group);
}
