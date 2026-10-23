using System.Net;
using System.Text.RegularExpressions;
using Xunit;

public class StorefrontViewTests : IClassFixture<StorefrontFactory>
{
    private readonly StorefrontFactory factory;

    public StorefrontViewTests(StorefrontFactory factory)
    {
        this.factory = factory;
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Catalog/Shop")]
    [InlineData("/Catalog/Details/000000000000000000000064")]
    [InlineData("/Catalog/Details/demo-1")]
    [InlineData("/Cart")]
    [InlineData("/Storefront/Blog")]
    [InlineData("/Storefront/Article/1")]
    [InlineData("/Storefront/Contact")]
    public async Task StorefrontPagesRenderOganiWithAttribution(string path)
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string html = await response.Content.ReadAsStringAsync();
        Assert.Contains("/vendor/ogani/css/style.css", html);
        Assert.Contains("https://colorlib.com", html);
        Assert.DoesNotContain("./shop-grid.html", html);
    }

    [Fact]
    public async Task MissingProductIdReturnsNotFound()
    {
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/Catalog/Details")).StatusCode);
    }

    [Fact]
    public async Task LoginKeepsSeparateAdminLayout()
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/Account/Login");
        response.EnsureSuccessStatusCode();
        string html = await response.Content.ReadAsStringAsync();
        Assert.Contains("/css/site.css", html);
        Assert.DoesNotContain("/vendor/ogani/css/style.css", html);
    }

    [Fact]
    public async Task RealProductCanReachCartAndCheckoutWithAntiforgery()
    {
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        string token = await GetToken(client);
        var response = await client.PostAsync("/Cart/Add", new FormUrlEncodedContent(new Dictionary<string, string> {
            ["productId"] = "000000000000000000000064", ["quantity"] = "2", ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        string cart = await client.GetStringAsync("/Cart");
        Assert.Contains("Tai nghe Nova", cart);
        Assert.Contains("action=\"/Cart/Update\"", cart);
        string checkout = await client.GetStringAsync("/Cart/Checkout");
        Assert.Contains("name=\"Address\"", checkout);
        Assert.Contains("name=\"CheckoutKey\"", checkout);
        Assert.Contains("name=\"__RequestVerificationToken\"", checkout);
    }

    [Fact]
    public async Task DemoProductsCannotEnterRealCart()
    {
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        string token = await GetToken(client);
        var response = await client.PostAsync("/Cart/Add", new FormUrlEncodedContent(new Dictionary<string, string> {
            ["productId"] = "demo-1", ["quantity"] = "1", ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Sản phẩm minh họa chưa mở bán.", await response.Content.ReadAsStringAsync());
    }

    private static async Task<string> GetToken(HttpClient client)
    {
        string html = await client.GetStringAsync("/Account/Login");
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success);
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }
}
