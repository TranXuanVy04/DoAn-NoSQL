using Ecommerce.Web.Models;

namespace Ecommerce.Web.Services;

public static class ProductSelection
{
    public static ProductVariant? Variant(Product product, string? variantId)
    {
        if (product.Variants.Count == 0)
        {
            if (!string.IsNullOrEmpty(variantId)) throw new BusinessException("Sản phẩm không có biến thể này.");
            return null;
        }
        return product.Variants.FirstOrDefault(v => v.Id == variantId && v.IsActive)
            ?? throw new BusinessException("Hãy chọn biến thể đang bán.");
    }

    public static OrderItem Snapshot(Product product, Category category, int quantity, string? variantId)
    {
        var variant = Variant(product, variantId);
        long price = variant?.Price ?? product.Price;
        return new OrderItem {
            ProductId=product.Id, Sku=variant?.Sku ?? product.Sku, Name=product.Name, CategoryName=category.Name,
            Attributes=product.Attributes.DeepClone().AsBsonDocument,
            Variant=variant == null ? null : new OrderVariant { Id=variant.Id, Sku=variant.Sku, Options=new(variant.Options) },
            UnitPrice=price, Quantity=quantity, LineTotal=checked(price*quantity)
        };
    }

    public static CartRow CartRow(Product product, int quantity, string? variantId)
    {
        var variant = Variant(product, variantId);
        if (variant == null) return new(product, quantity);
        var selected = new Product {
            Id = product.Id, Name = product.Name, Sku = variant.Sku, ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId, Attributes = product.Attributes,
            Price = variant.Price, Stock = variant.Stock, IsActive = product.IsActive && variant.IsActive
        };
        return new(selected, quantity, variant.Id, variant.Label);
    }
}
