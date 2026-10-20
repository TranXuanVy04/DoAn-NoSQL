using Ecommerce.Web.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;
namespace Ecommerce.Web.Services;

public sealed class MongoCatalogService(IMongoDatabase db) : ICatalogService {
    private IMongoCollection<Category> Cats => db.GetCollection<Category>("danhmuc");
    private IMongoCollection<Product> Goods => db.GetCollection<Product>("sanpham");
    public Task<List<Category>> Categories() => Cats.Find(FilterDefinition<Category>.Empty).SortBy(x=>x.Name).ToListAsync();
    public async Task SaveCategory(CategoryForm f) {
        if (f.Kind is not ("electronics" or "fashion" or "home")) throw new BusinessException("Ngành hàng không hợp lệ.");
        var name=f.Name.Trim();
        if (name.Length is <1 or >80) throw new BusinessException("Tên danh mục không hợp lệ.");
        var duplicate=await Cats.Find(x=>x.Name==name && x.Id!=f.Id).AnyAsync();
        if (duplicate) throw new BusinessException("Tên danh mục đã tồn tại.");
        if (f.Id is null) await Cats.InsertOneAsync(new Category {Name=name,Kind=f.Kind});
        else {
            var old=await Cats.Find(x=>x.Id==f.Id).FirstOrDefaultAsync() ?? throw new BusinessException("Danh mục không tồn tại.");
            if (old.Kind!=f.Kind && await Goods.Find(x=>x.CategoryId==f.Id).AnyAsync()) throw new BusinessException("Không đổi ngành khi danh mục còn sản phẩm.");
            await Cats.ReplaceOneAsync(x=>x.Id==f.Id,new Category {Id=f.Id,Name=name,Kind=f.Kind});
        }
    }
    public async Task DeleteCategory(string id) {
        if (!ObjectId.TryParse(id,out _)) throw new BusinessException("Mã danh mục không hợp lệ.");
        if (await Goods.Find(x=>x.CategoryId==id).AnyAsync()) throw new BusinessException("Danh mục còn sản phẩm; hãy chuyển sản phẩm sang danh mục khác trước.");
        await Cats.DeleteOneAsync(x=>x.Id==id);
    }
    public async Task<ProductPage> Products(string? query,string? categoryId,int page,bool admin=false) {
        var b=Builders<Product>.Filter; var filter=admin ? b.Empty : b.Eq(x=>x.IsActive,true);
        if (!string.IsNullOrWhiteSpace(query)) filter &= b.Regex(x=>x.Name,new BsonRegularExpression(Regex.Escape(query.Trim()[..Math.Min(query.Trim().Length,100)]),"i"));
        if (!string.IsNullOrEmpty(categoryId)) {
            if (!ObjectId.TryParse(categoryId,out _)) throw new BusinessException("Mã danh mục không hợp lệ.");
            filter &= b.Eq(x=>x.CategoryId,categoryId);
        }
        const int size=12; var total=await Goods.CountDocumentsAsync(filter);
        page=Math.Clamp(page,1,Math.Max(1,(int)Math.Ceiling(total/(double)size)));
        var rows=await Goods.Find(filter).SortBy(x=>x.Name).ThenBy(x=>x.Id).Skip((page-1)*size).Limit(size).ToListAsync();
        return new(rows,total,page,size);
    }
    public Task<Product?> Product(string id) => ObjectId.TryParse(id,out _) ? GetProduct(id) : Task.FromResult<Product?>(null);
    private async Task<Product?> GetProduct(string id) => await Goods.Find(x=>x.Id==id).FirstOrDefaultAsync();
    public async Task SaveProduct(ProductForm f) {
        if (!ObjectId.TryParse(f.CategoryId,out _)) throw new BusinessException("Hãy chọn danh mục.");
        var category=await Cats.Find(x=>x.Id==f.CategoryId).FirstOrDefaultAsync() ?? throw new BusinessException("Danh mục không tồn tại.");
        BsonDocument attributes;
        try { attributes=BsonDocument.Parse(f.AttributesJson); }
        catch (Exception ex) when (ex is FormatException or System.Text.Json.JsonException) { throw new BusinessException("Thuộc tính phải là một JSON object hợp lệ."); }
        ValidateAttributes(category.Kind,attributes);
        if (await Goods.Find(x=>x.Sku==f.Sku && x.Id!=f.Id).AnyAsync()) throw new BusinessException("SKU đã tồn tại.");
        if (f.Price is <1 or >1000000000000 || f.Stock is <0 or >1000000) throw new BusinessException("Giá/tồn kho không hợp lệ.");
        var p=new Product {Sku=f.Sku.Trim(),Name=f.Name.Trim(),CategoryId=f.CategoryId,Kind=category.Kind,Price=f.Price,Stock=f.Stock,IsActive=f.IsActive,Attributes=attributes};
        if (f.Id is null) await Goods.InsertOneAsync(p);
        else {
            if (!ObjectId.TryParse(f.Id,out _)) throw new BusinessException("Mã sản phẩm không hợp lệ.");
            p.Id=f.Id;
            var result=await Goods.ReplaceOneAsync(x=>x.Id==f.Id && x.Stock==f.OriginalStock,p);
            if (result.MatchedCount==0) throw new BusinessException("Sản phẩm đã thay đổi tồn kho hoặc không tồn tại. Hãy tải lại form trước khi sửa.");
        }
    }
    public static void ValidateAttributes(string kind,BsonDocument a) {
        string[] required=kind switch {"electronics"=>["brand","warrantyMonths"],"fashion"=>["size","material"],"home"=>["material","origin"],_=>throw new BusinessException("Ngành không hợp lệ.")};
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
        await Goods.UpdateOneAsync(x=>x.Id==id,Builders<Product>.Update.Set(x=>x.IsActive,false));
    }
}
