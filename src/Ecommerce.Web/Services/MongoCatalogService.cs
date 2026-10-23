using Ecommerce.Web.Models;
using MongoDB.Bson;
using Ecommerce.Web.Repositories;
namespace Ecommerce.Web.Services;

public sealed class MongoCatalogService : ICatalogService
{
    private readonly CategoryRepository categories;
    private readonly ProductRepository products;

    public MongoCatalogService(CategoryRepository categories, ProductRepository products)
    {
        this.categories = categories;
        this.products = products;
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

    public async Task SaveProduct(ProductForm f) {
        if (!ObjectId.TryParse(f.CategoryId,out _)) throw new BusinessException("Hãy chọn danh mục.");
        var category=await categories.GetById(f.CategoryId) ?? throw new BusinessException("Danh mục không tồn tại.");
        BsonDocument attributes;
        try { attributes=BsonDocument.Parse(f.AttributesJson); }
        catch (Exception ex) when (ex is FormatException or System.Text.Json.JsonException) { throw new BusinessException("Thuộc tính phải là một JSON object hợp lệ."); }
        string productKind = "general";
        if (f.Id != null)
        {
            if (!ObjectId.TryParse(f.Id, out _))
                throw new BusinessException("Mã sản phẩm không hợp lệ.");
            var existingProduct = await products.GetById(f.Id);
            if (existingProduct == null)
                throw new BusinessException("Sản phẩm không tồn tại.");
            productKind = existingProduct.Kind;
        }
        ValidateAttributes(productKind, attributes);
        if (await products.SkuExists(f.Sku, f.Id)) throw new BusinessException("SKU đã tồn tại.");
        if (f.Price is <1 or >1000000000000 || f.Stock is <0 or >1000000) throw new BusinessException("Giá/tồn kho không hợp lệ.");
        var p=new Product {Sku=f.Sku.Trim(),Name=f.Name.Trim(),CategoryId=f.CategoryId,Kind=productKind,Price=f.Price,Stock=f.Stock,IsActive=f.IsActive,Attributes=attributes};
        if (f.Id is null) await products.Create(p);
        else {
            if (!ObjectId.TryParse(f.Id,out _)) throw new BusinessException("Mã sản phẩm không hợp lệ.");
            p.Id=f.Id;
            bool updated = await products.Update(p, f.OriginalStock);
            if (!updated) throw new BusinessException("Sản phẩm đã thay đổi tồn kho hoặc không tồn tại. Hãy tải lại form trước khi sửa.");
        }
    }
    public static void ValidateAttributes(string kind,BsonDocument a) {
        string[] required=kind switch {"electronics"=>["brand","warrantyMonths"],"fashion"=>["size","material"],"home"=>["material","origin"],_=>[]};
        foreach(var field in required) if (!a.TryGetValue(field,out var value) || value.IsBsonNull || (value.IsString && string.IsNullOrWhiteSpace(value.AsString)))
            throw new BusinessException($"Thiếu thuộc tính {field} cho ngành {kind}.");
        if(kind=="electronics" && (!a["warrantyMonths"].IsNumeric || a["warrantyMonths"].ToDouble()<0)) throw new BusinessException("warrantyMonths phải là số không âm.");
        foreach(var field in required.Where(x=>x!="warrantyMonths")) if (!a[field].IsString) throw new BusinessException($"{field} phải là chuỗi.");
        if(a.ToJson().Length>6000 || ContainsUnsafe(a)) throw new BusinessException("Thuộc tính quá dài hoặc chứa khóa không hợp lệ.");
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
