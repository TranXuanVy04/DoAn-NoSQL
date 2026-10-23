using System.Net;
using System.Text.Json;
using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using Xunit;

public sealed class CatalogApiTests : IClassFixture<CatalogApiFactory> {
    private readonly HttpClient client;
    public CatalogApiTests(CatalogApiFactory factory) => client = factory.CreateClient(new() { AllowAutoRedirect = false });
    [Fact] public async Task ProductAttributesArePlainJsonAndMoneyIsExact() {
        var response = await client.GetAsync("/api/products");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var product = json.RootElement.GetProperty("items")[0];
        Assert.Equal(350000L, product.GetProperty("price").GetInt64());
        Assert.Equal("Nova", product.GetProperty("attributes").GetProperty("brand").GetString());
        Assert.Equal(12, product.GetProperty("attributes").GetProperty("warrantyMonths").GetInt32());
    }
    [Theory]
    [InlineData("/api/products/bad-id", 400)]
    [InlineData("/api/products?page=0", 400)]
    [InlineData("/api/products?categoryId=bad", 400)]
    [InlineData("/api/categories/bad-id", 400)]
    [InlineData("/api/analytics/summary?from=2026-08-01&to=2026-10-31", 401)]
    public async Task ErrorsUseProblemJson(string path, int status) {
        var response = await client.GetAsync(path);
        Assert.Equal(status, (int)response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
public sealed class CatalogApiFactory : WebApplicationFactory<Program> {
    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Mongo:ConnectionString", "mongodb://localhost:27017");
        builder.UseSetting("Mongo:Database", "ecommerce_test");
        builder.ConfigureServices(services => {
            services.RemoveAll<ICatalogService>();
            services.AddSingleton<ICatalogService, FakeCatalog>();
            services.RemoveAll<IDashboardService>();
            services.AddSingleton<IDashboardService, FakeDashboard>();
        });
    }
}
public sealed class FakeDashboard : IDashboardService {
    public Task<DashboardView> Report(DateOnly from, DateOnly to, string group) => Task.FromResult(new DashboardView(from, to, group, [new("2026-08-01", 350000, 1)], [new("000000000000000000000064", "Tai nghe Nova", 1, 350000)]));
}
public sealed class FakeCatalog : ICatalogService {
    private static Product Sample() => new() { Id = "000000000000000000000064", Sku = "ELEC-001", Name = "Tai nghe Nova", CategoryId = "000000000000000000000001", Price = 350000, Stock = 20, Attributes = new BsonDocument { { "brand", "Nova" }, { "warrantyMonths", 12 } } };
    public Task<List<Category>> Categories() => Task.FromResult(new List<Category> { new() { Id = "000000000000000000000001", Name = "Điện tử", ParentId = null } });
    public Task<ProductPage> Products(string? query, string? categoryId, int page, bool admin = false) => Task.FromResult(new ProductPage([Sample()], 1, page, 12));
    public Task<Product?> Product(string id) => Task.FromResult<Product?>(id == Sample().Id ? Sample() : null);
    public Task<Category> SaveCategory(CategoryForm input) => throw new NotSupportedException();
    public Task DeleteCategory(string id) => throw new NotSupportedException();
    public Task SaveProduct(ProductForm input) => throw new NotSupportedException();
    public Task DeleteProduct(string id) => throw new NotSupportedException();
}
