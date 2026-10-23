using System.Net;
using System.Text.RegularExpressions;
using Xunit;

public class AdminAuthorizationTests
{
    [Fact]
    public async Task AuthenticatedCustomerCannotChangeOrderStatus()
    {
        using var factory = new CategoryCrudFactory();
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Test-Customer", "yes");
        var html = await client.GetStringAsync("/Account/Login");
        var token = WebUtility.HtmlDecode(Regex.Match(html,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
        Assert.False(string.IsNullOrEmpty(token));
        foreach (var status in new[] { "Completed", "Cancelled" })
        {
            var response = await client.PostAsync("/Admin/OrderStatus", new FormUrlEncodedContent(new Dictionary<string,string> {
                ["id"]="000000000000000000000001", ["status"]=status, ["__RequestVerificationToken"]=token
            }));
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/Admin/Orders")).StatusCode);
    }
}
