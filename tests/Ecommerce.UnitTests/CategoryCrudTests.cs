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
    [Fact]
    public async Task AdminCanCreateReadUpdateAndDelete()
    {
        using var factory = new CategoryCrudFactory();
        using var client = await AdminClient(factory);
        var response = await client.PostAsJsonAsync("/api/categories", new { name = "Danh mục mới", parentId = (string?)null });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var category = (await response.Content.ReadFromJsonAsync<Category>())!;
        Assert.EndsWith("/api/categories/" + category.Id, response.Headers.Location!.ToString());
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(response.Headers.Location)).StatusCode);
        var updated = await client.PutAsJsonAsync("/api/categories/" + category.Id, new { name = "Tên đã sửa", parentId = (string?)null });
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal("Tên đã sửa", (await updated.Content.ReadFromJsonAsync<Category>())!.Name);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync("/api/categories/" + category.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/categories/" + category.Id)).StatusCode);
    }

    [Theory]
    [InlineData("POST", "/api/categories")]
    [InlineData("PUT", "/api/categories/000000000000000000000001")]
    [InlineData("DELETE", "/api/categories/000000000000000000000001")]
    public async Task GuestCannotWrite(string method, string path)
    {
        using var factory = new CategoryCrudFactory();
        using var client = factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod(method), path);
        request.Content = JsonContent.Create(new { name = "Test", parentId = (string?)null });
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.SendAsync(request)).StatusCode);
    }

    [Fact]
    public async Task AdminStillNeedsCsrfToken()
    {
        using var factory = new CategoryCrudFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Admin", "yes");
        var response = await client.PostAsJsonAsync("/api/categories", new { name = "Test", parentId = (string?)null });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("Test", "bad-id")]
    public async Task InvalidInputIsRejected(string name, string? parentId)
    {
        using var factory = new CategoryCrudFactory();
        using var client = await AdminClient(factory);
        var response = await client.PostAsJsonAsync("/api/categories", new { name, parentId });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DuplicateNameAndReferencedCategoryReturnConflict()
    {
        using var factory = new CategoryCrudFactory();
        using var client = await AdminClient(factory);
        var response = await client.PostAsJsonAsync("/api/categories", new { name = "Điện tử", parentId = (string?)null });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync("/api/categories/000000000000000000000001")).StatusCode);
        response = await client.PutAsJsonAsync("/api/categories/000000000000000000000001", new { name = "Điện tử", parentId = (string?)null });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("bad-id", 400)]
    [InlineData("000000000000000000000099", 404)]
    public async Task UpdateAndDeleteCheckId(string id, int status)
    {
        using var factory = new CategoryCrudFactory();
        using var client = await AdminClient(factory);
        var response = await client.PutAsJsonAsync("/api/categories/" + id, new { name = "Test", parentId = (string?)null });
        Assert.Equal(status, (int)response.StatusCode);
        Assert.Equal(status, (int)(await client.DeleteAsync("/api/categories/" + id)).StatusCode);
    }

    private static async Task<HttpClient> AdminClient(CategoryCrudFactory factory)
    {
        var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Test-Admin", "yes");
        var response = await client.GetAsync("/api/categories/csrf");
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", json.RootElement.GetProperty("token").GetString());
        return client;
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
        if (Request.Headers["X-Test-Admin"] != "yes")
            return Task.FromResult(AuthenticateResult.NoResult());
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test-admin"), new Claim(ClaimTypes.Role, "Admin") }, "Test");
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
