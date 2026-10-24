using System.Text.Json;
using System.Text.RegularExpressions;
using Ecommerce.Web.Models;
using MongoDB.Bson;

namespace Ecommerce.Web.Services;

// Chuyển form quản trị thành dữ liệu sản phẩm; không truy vấn database.
public static class ProductFormMapper
{
    public static Product Build(ProductForm form, Product? existing, IReadOnlyDictionary<string, string> rules)
    {
        string sku = (form.Sku ?? "").Trim();
        string name = (form.Name ?? "").Trim();
        if (!ValidSku(sku)) throw new BusinessException("SKU phải gồm 1–40 ký tự chữ, số, dấu - hoặc _.");
        if (name.Length is < 1 or > 160) throw new BusinessException("Tên sản phẩm phải từ 1 đến 160 ký tự.");
        if (existing != null && (existing.Variants.Count > 0) != form.HasVariants)
            throw new BusinessException("Không đổi loại sản phẩm sau khi tạo. Hãy tạo sản phẩm mới để giữ đúng đơn hàng cũ.");
        string image = form.ImageUrl?.Trim() ?? "";
        bool localImage = Regex.IsMatch(image, @"^/uploads/products/[a-f0-9]{32}\.(jpg|png|webp)$");
        if (image.Length > 2000 || image.Length > 0 && !localImage && (!Uri.TryCreate(image, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")))
            throw new BusinessException("Đường dẫn ảnh phải là URL http hoặc https hợp lệ.");
        BsonDocument attributes;
        try { attributes = BsonDocument.Parse(form.AttributesJson ?? ""); }
        catch (Exception ex) when (ex is FormatException or JsonException) { throw new BusinessException("Thuộc tính chung phải là JSON object hợp lệ."); }
        var product = new Product {
            Id=existing?.Id ?? ObjectId.GenerateNewId().ToString(), Sku=sku, Name=name,
            CategoryId=form.CategoryId, ImageUrl=image, IsActive=form.IsActive,
            Attributes=attributes, Revision=(existing?.Revision ?? -1)+1
        };
        MongoCatalogService.ValidateAttributes(new Dictionary<string,string>(), attributes);
        if (!form.HasVariants)
        {
            ValidateMoneyStock(form.Price, form.Stock);
            MongoCatalogService.ValidateAttributes(rules, attributes);
            product.Price=form.Price; product.Stock=form.Stock;
            return product;
        }
        if (form.Variants == null || form.Variants.Count is < 1 or > 100)
            throw new BusinessException("Sản phẩm phải có từ 1 đến 100 biến thể.");
        var ids = new HashSet<string>();
        var skus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var combinations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in form.Variants)
        {
            string id = string.IsNullOrWhiteSpace(row.Id) ? Guid.NewGuid().ToString("N") : row.Id.Trim();
            if (!Regex.IsMatch(id, "^[A-Za-z0-9_-]{1,80}$") || !ids.Add(id))
                throw new BusinessException("ID biến thể không hợp lệ hoặc bị trùng.");
            string variantSku=(row.Sku ?? "").Trim();
            if (!ValidSku(variantSku) || !skus.Add(variantSku))
                throw new BusinessException("SKU biến thể không hợp lệ hoặc bị trùng.");
            var options = ParseOptions(row);
            string combination = JsonSerializer.Serialize(options.OrderBy(o=>o.Key).ToDictionary(o=>o.Key,o=>o.Value));
            if (!combinations.Add(combination)) throw new BusinessException("Hai biến thể không được cùng tổ hợp thuộc tính màu, size.");
            ValidateMoneyStock(row.Price,row.Stock);
            var combined = attributes.DeepClone().AsBsonDocument;
            foreach (var option in options) combined[option.Key]=option.Value;
            MongoCatalogService.ValidateAttributes(rules,combined);
            product.Variants.Add(new ProductVariant {Id=id,Sku=variantSku,Options=options,Price=row.Price,Stock=row.Stock,IsActive=row.IsActive});
        }
        // Giữ ID của biến thể cũ để hủy đơn có thể hoàn đúng kho.
        if (existing != null && existing.Variants.Any(v=>!ids.Contains(v.Id)))
            throw new BusinessException("Không xóa biến thể đã lưu. Bỏ chọn Đang bán để ngừng bán biến thể đó.");
        var keys=product.Variants[0].Options.Keys.Order().ToArray();
        if (product.Variants.Any(v=>!v.Options.Keys.Order().SequenceEqual(keys)))
            throw new BusinessException("Các biến thể phải có cùng nhóm thuộc tính, ví dụ đều có màu và size.");
        return product;
    }

    private static Dictionary<string,string> ParseOptions(VariantForm row)
    {
        Dictionary<string,string> options;
        try { options=JsonSerializer.Deserialize<Dictionary<string,string>>(row.OptionsJson ?? "{}") ?? throw new JsonException(); }
        catch (JsonException) { throw new BusinessException("Thuộc tính bổ sung của biến thể phải là JSON object với giá trị chuỗi."); }
        if (!string.IsNullOrWhiteSpace(row.Color)) options["color"]=row.Color.Trim();
        if (!string.IsNullOrWhiteSpace(row.Size)) options["size"]=row.Size.Trim();
        if (options.Count == 0 || options.Count > 20 || options.Keys.Select(k=>k.Trim()).Distinct().Count()!=options.Count || options.Any(o=>string.IsNullOrWhiteSpace(o.Key) || o.Key.StartsWith('$') || o.Key.Contains('.') || string.IsNullOrWhiteSpace(o.Value) || o.Value.Length > 160 || o.Key.Length > 80))
            throw new BusinessException("Mỗi biến thể cần màu, size hoặc thuộc tính lựa chọn hợp lệ.");
        return options.ToDictionary(o=>o.Key.Trim(),o=>o.Value.Trim());
    }

    private static bool ValidSku(string sku) => Regex.IsMatch(sku,"^[A-Za-z0-9_-]{1,40}$");
    private static void ValidateMoneyStock(long price,int stock)
    {
        if (price is < 1 or > 1000000000000 || stock is < 0 or > 1000000)
            throw new BusinessException("Giá phải lớn hơn 0; tồn kho từ 0 đến 1.000.000.");
    }
}
