using Ecommerce.Web.Models;
using MongoDB.Bson;
using Ecommerce.Web.Repositories;
namespace Ecommerce.Web.Services;

public sealed class MongoCatalogService : ICatalogService
{
    private readonly CategoryRepository categories;
    private readonly ProductRepository products;
    private readonly IConfiguration configuration;

    public MongoCatalogService(CategoryRepository categories, ProductRepository products, IConfiguration configuration)
    {
        this.categories = categories;
        this.products = products;
        this.configuration = configuration;
    }

    public Task<List<Category>> Categories()
    {
        return categories.GetAll();
    }

    public async Task<Category> SaveCategory(CategoryForm input)
    {
string name = input.Name.Trim();
        if (name.Length < 1 || name.Length > 80)
            throw new BusinessException("Tên danh mục phải từ 1 đến 80 ký tự.");

        input.ParentId = string.IsNullOrWhiteSpace(input.ParentId) ? null : input.ParentId.ToLowerInvariant();
        var allCategories = await categories.GetAll();
        CategoryHierarchy.ValidateParent(input.Id, input.ParentId, allCategories);

        bool nameExists = await categories.NameExists(name, input.Id);
        if (nameExists)
            throw new BusinessException("Tên danh mục đã tồn tại.");

        if (input.Id == null)
        {
            var category = new Category { Name = name, ParentId = input.ParentId };
            await categories.Create(category);
            return category;
        }

        var existing = await categories.GetById(input.Id);
        if (existing == null)
            throw new BusinessException("Danh mục không tồn tại.");

existing.Name = name;
        existing.ParentId = input.ParentId;
        await categories.Update(existing);
        return existing;
    }
    public async Task DeleteCategory(string id) {
        if (!ObjectId.TryParse(id,out _)) throw new BusinessException("Mã danh mục không hợp lệ.");
        if (await categories.HasChildren(id))
            throw new BusinessException("Danh mục còn danh mục con; hãy chuyển hoặc xóa danh mục con trước.");
        if (await products.HasCategory(id)) throw new BusinessException("Danh mục còn sản phẩm; hãy chuyển sản phẩm sang danh mục khác trước.");
        await categories.Delete(id);
    }
    public Task<ProductPage> Products(string? query, string? categoryId, int page, bool admin = false)
    {
        if (!string.IsNullOrEmpty(categoryId) && !ObjectId.TryParse(categoryId, out _))
            throw new BusinessException("Mã danh mục không hợp lệ.");
        return products.GetPage(query, categoryId, page, admin);
    }

    public Task<Product?> Product(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return Task.FromResult<Product?>(null);
        return products.GetById(id);
    }

    public async Task SaveProduct(ProductForm form)
    {
        if (!ObjectId.TryParse(form.CategoryId,out _)) throw new BusinessException("Hãy chọn danh mục.");
        Product? existing=null;
        if (form.Id != null)
        {
            if (!ObjectId.TryParse(form.Id,out _)) throw new BusinessException("Mã sản phẩm không hợp lệ.");
            existing=await products.GetById(form.Id) ?? throw new BusinessException("Sản phẩm không tồn tại.");
            if (existing.Revision != form.OriginalRevision)
                throw new BusinessException("Sản phẩm hoặc tồn kho vừa thay đổi. Hãy tải lại form.");
        }
        var root=CategoryHierarchy.GetRoot(form.CategoryId, await categories.GetAll());
        var rules=configuration.GetSection("CategoryAttributeRules:"+root.Id).Get<Dictionary<string,string>>() ?? new();
        var product=ProductFormMapper.Build(form,existing,rules);
        var skus=product.Variants.Select(v=>v.Sku).Append(product.Sku).Distinct().ToArray();
        if (await products.AnySkuExists(skus,form.Id)) throw new BusinessException("SKU sản phẩm hoặc biến thể đã được sản phẩm khác sử dụng.");
        if (existing == null) await products.Create(product);
        else if (!await products.Update(product,form.OriginalRevision))
            throw new BusinessException("Sản phẩm hoặc tồn kho vừa thay đổi. Hãy tải lại form trước khi sửa.");
    }
    public static void ValidateAttributes(IReadOnlyDictionary<string, string> rules, BsonDocument attributes)
    {
        foreach (var rule in rules)
        {
            if (!attributes.TryGetValue(rule.Key, out var value) || value.IsBsonNull)
                throw new BusinessException($"Thiếu thuộc tính {rule.Key} theo danh mục gốc.");
            if (rule.Value == "string" && (!value.IsString || string.IsNullOrWhiteSpace(value.AsString)))
                throw new BusinessException($"{rule.Key} phải là chuỗi không rỗng.");
            if (rule.Value == "nonNegativeNumber" && (!value.IsNumeric || !double.IsFinite(value.ToDouble()) || value.ToDouble() < 0))
                throw new BusinessException($"{rule.Key} phải là số không âm.");
        }
        if (attributes.ToJson().Length > 6000 || ContainsUnsafe(attributes))
            throw new BusinessException("Thuộc tính quá dài hoặc chứa khóa không hợp lệ.");
    }
    private static bool ContainsUnsafe(BsonValue v) => v switch {
        BsonDocument d => d.Elements.Any(e=>e.Name.StartsWith('$') || e.Name.Contains('.') || ContainsUnsafe(e.Value)),
        BsonArray a => a.Any(ContainsUnsafe), _ => false
    };
    public async Task DeleteProduct(string id) {
        if (!ObjectId.TryParse(id,out _)) throw new BusinessException("Mã sản phẩm không hợp lệ.");
        // Soft delete: giữ tham chiếu cho tồn kho/đơn cũ; catalog ẩn sản phẩm.
        await products.Deactivate(id);
    }
}
