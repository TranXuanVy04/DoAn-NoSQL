using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
namespace Ecommerce.Web.Models;

public sealed class CategoryForm {
    public string? Id { get; set; }

    [Required, StringLength(80)] 
    public string Name { get; set; } = "";

    [RegularExpression("^[0-9a-fA-F]{24}$")]
    public string? ParentId { get; set; }
}
public sealed class ProductForm {
    [StringLength(2000)]
    public string? ImageUrl { get; set; }
    public string? Id { get; set; }
    public long OriginalRevision { get; set; }
    public bool HasVariants { get; set; }
    public List<VariantForm> Variants { get; set; } = [];
    [Required, StringLength(40), RegularExpression("^[A-Za-z0-9_-]+$")] public string Sku { get; set; } = "";
    [Required, StringLength(160)] public string Name { get; set; } = "";
    [Required] public string CategoryId { get; set; } = "";
    [Range(0, 1000000000000)] public long Price { get; set; }
    [Range(0, 1000000)] public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
    [Required, StringLength(6000)] public string AttributesJson { get; set; } = "{}";
    public static ProductForm From(Product p) => new() { Id=p.Id, ImageUrl=p.ImageUrl, Sku=p.Sku, Name=p.Name, CategoryId=p.CategoryId,
        Price=p.Price, Stock=p.Stock, OriginalRevision=p.Revision, HasVariants=p.Variants.Count>0,
        Variants=p.Variants.Select(v=>new VariantForm {Id=v.Id,Sku=v.Sku,Color=v.Options.GetValueOrDefault("color"),Size=v.Options.GetValueOrDefault("size"),OptionsJson=System.Text.Json.JsonSerializer.Serialize(v.Options.Where(o=>o.Key!="color" && o.Key!="size").ToDictionary()),Price=v.Price,Stock=v.Stock,IsActive=v.IsActive}).ToList(), IsActive=p.IsActive, AttributesJson=p.Attributes.ToJson() };
}
public sealed class VariantForm {
    public string? Id { get; set; }
    public string? Sku { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string OptionsJson { get; set; } = "{}";
    public long Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
}
public sealed class CheckoutForm {
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Required, RegularExpression("^[0-9+ ()-]{9,20}$")] public string Phone { get; set; } = "";
    [Required, StringLength(300, MinimumLength=10)] public string Address { get; set; } = "";
    [Required] public string CheckoutKey { get; set; } = "";
}
public sealed class LoginForm {
    [Required] public string Username { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
}
