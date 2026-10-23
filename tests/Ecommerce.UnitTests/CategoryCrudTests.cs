using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

public class CategoryCrudTests
{
    private static async Task<string> Token(HttpClient client)
    {
        var html = await client.GetStringAsync("/Admin/CategoryEdit");
        var match = System.Text.RegularExpressions.Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success);
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    [Fact]
    public async Task AdminCanCreateUpdateAndDeleteUsingRazorForms()
    {
        using var factory = new CategoryCrudFactory();
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Test-Admin", "yes");
        string token = await Token(client);
        async Task<HttpResponseMessage> Post(string path, Dictionary<string,string> values) {
            values["__RequestVerificationToken"] = token;
            return await client.PostAsync(path, new FormUrlEncodedContent(values));
        }
        Assert.Equal(HttpStatusCode.Redirect, (await Post("/Admin/CategoryEdit", new() { ["Name"] = "Sách" })).StatusCode);
        var service = factory.Services.GetRequiredService<ICatalogService>();
        var created = (await service.Categories()).Single(c => c.Name == "Sách");
        Assert.Equal(HttpStatusCode.Redirect, (await Post("/Admin/CategoryEdit", new() { ["Id"] = created.Id, ["Name"] = "Sách mới" })).StatusCode);
        Assert.Contains("Sách mới", WebUtility.HtmlDecode(await client.GetStringAsync("/Admin/Categories")));
        Assert.Equal(HttpStatusCode.Redirect, (await Post("/Admin/CategoryDelete", new() { ["id"] = created.Id })).StatusCode);
        Assert.DoesNotContain(await service.Categories(), c => c.Id == created.Id);
    }

    [Fact]
    public async Task AdminFormStillRequiresAntiforgery()
    {
        using var factory = new CategoryCrudFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Admin", "yes");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/Admin/CategoryEdit", new FormUrlEncodedContent(new Dictionary<string,string> { ["Name"] = "Sách" }))).StatusCode);
    }

    [Fact]
    public async Task GuestCannotOpenAdminForm()
    {
        using var factory = new StorefrontFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var response = await client.GetAsync("/Admin/CategoryEdit");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location!.ToString());
    }
}

public class CategoryCrudFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Mongo:ConnectionString", "mongodb://localhost:27017");
        builder.UseSetting("Mongo:Database", "ecommerce_test");
        builder.ConfigureServices(services => {
            services.RemoveAll<ICatalogService>();
            services.AddSingleton<ICatalogService, CategoryMemoryService>();
            services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, CategoryTestAuth>("Test", _ => { });
        });
    }
}

// Chỉ đăng ký trong test, không dùng trong ứng dụng thật.
public class CategoryTestAuth : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public CategoryTestAuth(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers["X-Test-Admin"] != "yes" && Request.Headers["X-Test-Customer"] != "yes")
            return Task.FromResult(AuthenticateResult.NoResult());
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test-admin"), new Claim(ClaimTypes.Role, Request.Headers["X-Test-Customer"] == "yes" ? "Customer" : "Admin") }, "Test");
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), "Test")));
    }
}

public class CategoryMemoryService : ICatalogService
{
    private readonly List<Category> categories = new() {
        new Category { Id = "000000000000000000000001", Name = "Điện tử", ParentId = null }
    };

    public Task<List<Category>> Categories() => Task.FromResult(categories.ToList());
    public Task<Category> SaveCategory(CategoryForm input)
    {
        string name = input.Name.Trim();
        CategoryHierarchy.ValidateParent(input.Id, input.ParentId, categories);
        if (categories.Any(x => x.Name == name && x.Id != input.Id))
            throw new BusinessException("Tên danh mục đã tồn tại.");
        if (input.Id == null)
        {
            var category = new Category { Name = name, ParentId = input.ParentId };
            categories.Add(category);
            return Task.FromResult(category);
        }
        var existing = categories.Single(x => x.Id == input.Id);
existing.Name = name;
        existing.ParentId = input.ParentId;
        return Task.FromResult(existing);
    }

    public Task DeleteCategory(string id)
    {
        if (id == "000000000000000000000001" || categories.Any(x => x.ParentId == id))
            throw new BusinessException("Danh mục còn sản phẩm.");
        categories.RemoveAll(x => x.Id == id);
        return Task.CompletedTask;
    }

    public Task<ProductPage> Products(string? query, string? categoryId, int page, bool admin = false) => throw new NotSupportedException();
    public Task<Product?> Product(string id) => throw new NotSupportedException();
    public Task SaveProduct(ProductForm input) => throw new NotSupportedException();
    public Task DeleteProduct(string id) => throw new NotSupportedException();
}
