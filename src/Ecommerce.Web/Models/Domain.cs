using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ecommerce.Web.Models;

[BsonIgnoreExtraElements]
public sealed class Category {
    [BsonId, BsonRepresentation(BsonType.ObjectId)] 
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string Name { get; set; } = "";
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ParentId { get; set; }
}
public sealed class Product {
    [BsonId, BsonRepresentation(BsonType.ObjectId)] public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    [BsonRepresentation(BsonType.ObjectId)] public string CategoryId { get; set; } = "";
    public string Kind { get; set; } = "electronics";
    public long Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
    public BsonDocument Attributes { get; set; } = new();
}
public sealed class Customer {
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
}
public sealed class OrderItem {
    [BsonRepresentation(BsonType.ObjectId)] public string ProductId { get; set; } = "";
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string Kind { get; set; } = "";
    public BsonDocument Attributes { get; set; } = new();
    public long UnitPrice { get; set; }
    public int Quantity { get; set; }
    public long LineTotal { get; set; }
}
public sealed class Order {
    [BsonId, BsonRepresentation(BsonType.ObjectId)] public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string Number { get; set; } = "";
    public string CheckoutKey { get; set; } = "";
    public string CustomerToken { get; set; } = "";
    public Customer Customer { get; set; } = new();
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";
    public List<OrderItem> Items { get; set; } = [];
    public long Total { get; set; }
}
public sealed record CartLine(string ProductId, int Quantity);
public sealed record CartRow(Product Product, int Quantity) { public long Total => checked(Product.Price * Quantity); }
public sealed record CartView(List<CartRow> Rows) { public long Total => Rows.Sum(x => x.Total); }
public sealed record ProductPage(List<Product> Items, long Total, int Page, int PageSize) {
    public int Pages => Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
}
public sealed record OrderPage(List<Order> Items, long Total, int Page, int PageSize) {
    public int Pages => Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
}
public sealed record RevenuePoint(string Period, long Revenue, int Orders);
public sealed record TopProduct(string ProductId, string Name, long Quantity, long Revenue);
public sealed record DashboardView(DateOnly From, DateOnly To, string Group, List<RevenuePoint> Revenue, List<TopProduct> Top);
public sealed class BusinessException(string message) : Exception(message);
