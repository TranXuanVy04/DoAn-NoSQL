using Ecommerce.Web.Models;

namespace Ecommerce.Web.Services;

public class DemoCatalogService : ICatalogService
{
    public Task<List<Category>> Categories() => Task.FromResult(StorefrontDemo.Categories);
    public Task<Product?> Product(string id) => Task.FromResult(StorefrontDemo.Products.FirstOrDefault(x => x.Id == id));
    public Task<ProductPage> Products(string? query, string? categoryId, int page, bool admin = false)
    {
        var rows = StorefrontDemo.Products.Where(x => string.IsNullOrWhiteSpace(query) || x.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(categoryId)) rows = rows.Where(x => x.CategoryId == categoryId);
        var items = rows.ToList();
        return Task.FromResult(new ProductPage(items, items.Count, 1, 12));
    }
    public Task<Category> SaveCategory(CategoryForm input) => throw new BusinessException("Chế độ minh họa không lưu dữ liệu.");
    public Task DeleteCategory(string id) => throw new BusinessException("Chế độ minh họa không lưu dữ liệu.");
    public Task SaveProduct(ProductForm input) => throw new BusinessException("Chế độ minh họa không lưu dữ liệu.");
    public Task DeleteProduct(string id) => throw new BusinessException("Chế độ minh họa không lưu dữ liệu.");
}
