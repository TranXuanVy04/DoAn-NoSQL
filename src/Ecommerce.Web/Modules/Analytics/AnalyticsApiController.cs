using Ecommerce.Web.Services;
using Ecommerce.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Ecommerce.Web.Modules.Analytics;

[ApiController, Authorize, Route("api/analytics")]
public sealed class AnalyticsApiController(IDashboardService dashboard) : ControllerBase
{
    [HttpGet("{metric:regex(^(revenue|top-products|summary)$)}")]
    public async Task<IActionResult> Get(string metric, [FromQuery] DateOnly from, [FromQuery] DateOnly to, string group = "day") {
        if (!Request.Query.ContainsKey("from") || !Request.Query.ContainsKey("to"))
            return Problem(statusCode:400, title:"Cần from/to dạng yyyy-MM-dd");
        try {
            var result = await dashboard.Report(from, to, group);
            return metric switch {
                "revenue" => Ok(result.Revenue),
                "top-products" => Ok(result.Top),
                _ => Ok(new { Revenue = result.Revenue.Sum(x => x.Revenue), Orders = result.Revenue.Sum(x => x.Orders), Timezone = "Asia/Ho_Chi_Minh" })
            };
        } catch (BusinessException ex) { return Problem(statusCode:400, title:ex.Message); }
    }
}
