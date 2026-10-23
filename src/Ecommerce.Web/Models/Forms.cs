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
    public string? Id { get; set; }
    public int OriginalStock { get; set; }
    [Required, StringLength(40), RegularExpression("^[A-Za-z0-9_-]+$")] public string Sku { get; set; } = "";
    [Required, StringLength(160)] public string Name { get; set; } = "";
    [Required] public string CategoryId { get; set; } = "";
    [Range(1, 1000000000000)] public long Price { get; set; }
    [Range(0, 1000000)] public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
    [Required, StringLength(6000)] public string AttributesJson { get; set; } = "{}";
    public static ProductForm From(Product p) => new() { Id=p.Id, Sku=p.Sku, Name=p.Name, CategoryId=p.CategoryId,
        Price=p.Price, Stock=p.Stock, OriginalStock=p.Stock, IsActive=p.IsActive, AttributesJson=p.Attributes.ToJson() };
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
