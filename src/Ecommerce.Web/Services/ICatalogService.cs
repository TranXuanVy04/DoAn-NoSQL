using Ecommerce.Web.Models;

namespace Ecommerce.Web.Services;

public interface ICatalogService {
    Task<List<Category>> Categories();
    Task<Category> SaveCategory(CategoryForm input);
    Task DeleteCategory(string id);
    Task<ProductPage> Products(string? query, string? categoryId, int page, bool admin = false);
    Task<Product?> Product(string id);
    Task SaveProduct(ProductForm input);
    Task DeleteProduct(string id);
}
