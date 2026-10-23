using MongoDB.Bson;

namespace Ecommerce.Web.Models;

// Chỉ dùng làm nội dung minh họa giao diện, không lưu vào MongoDB.
public static class StorefrontDemo
{
    public static List<Category> Categories => new()
    {
        new Category { Id = "000000000000000000000001", Name = "Điện tử" },
        new Category { Id = "000000000000000000000002", Name = "Thời trang" },
        new Category { Id = "000000000000000000000003", Name = "Gia dụng" }
    };

    public static List<Product> Products => new()
    {
        MakeProduct(1, "Tai nghe không dây", 350000, "electronics"),
        MakeProduct(2, "Áo thun cotton", 180000, "fashion"),
        MakeProduct(3, "Bình giữ nhiệt", 240000, "home"),
        MakeProduct(4, "Loa Bluetooth mini", 590000, "electronics"),
        MakeProduct(5, "Túi canvas", 150000, "fashion"),
        MakeProduct(6, "Đèn để bàn", 320000, "home"),
        MakeProduct(7, "Bàn phím không dây", 650000, "electronics"),
        MakeProduct(8, "Bộ hộp bảo quản", 220000, "home")
    };

    public static List<StorefrontArticle> Articles => new()
    {
        new StorefrontArticle(1, "Những lựa chọn nhỏ cho một ngày tốt hơn", "Tìm cảm hứng mua sắm từ những vật dụng gần gũi trong cuộc sống.", "blog-1.jpg"),
        new StorefrontArticle(2, "Sắp xếp không gian, làm mới cảm hứng", "Một góc nhà gọn gàng có thể mang đến nhiều năng lượng tích cực.", "blog-2.jpg"),
        new StorefrontArticle(3, "Mua sắm thông minh theo nhu cầu", "Gợi ý lựa chọn sản phẩm phù hợp với thói quen và ngân sách của bạn.", "blog-3.jpg")
    };

    public static bool IsDemo(Product product) => product.Id.StartsWith("demo-");

    public static string ProductImage(Product product)
    {
        if (Uri.TryCreate(product.ImageUrl, UriKind.Absolute, out var imageUri)
            && (imageUri.Scheme == Uri.UriSchemeHttp || imageUri.Scheme == Uri.UriSchemeHttps))
            return product.ImageUrl;
        if (!IsDemo(product)) return "/img/product-placeholder.svg";
        int number = product.Id.Aggregate(0, (sum, c) => sum + c) % 8 + 1;
        return "/vendor/ogani/img/featured/feature-" + number + ".jpg";
    }

    private static Product MakeProduct(int number, string name, long price, string kind)
    {
        return new Product {
            Id = "demo-" + number, Sku = "DEMO-00" + number, Name = name,
            CategoryId = Categories.First(x => x.Name == (kind == "electronics" ? "Điện tử" : kind == "fashion" ? "Thời trang" : "Gia dụng")).Id,
            Price = price, Stock = 20,
            Attributes = new BsonDocument { { "Thông tin", "Sản phẩm minh họa, sẽ cập nhật sau" } }
        };
    }
}

public class StorefrontHeaderViewModel
{
    public List<Category> Categories { get; set; } = new();
    public int CartQuantity { get; set; }
}

public record StorefrontArticle(int Id, string Title, string Description, string Image);
