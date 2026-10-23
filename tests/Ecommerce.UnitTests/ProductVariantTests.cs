using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using MongoDB.Bson;
using Xunit;

public class ProductVariantTests
{
    private static Product Shirt() => new() {
        Name = "Áo thun", Sku = "AO", CategoryId = "000000000000000000000002", Variants = [
            new() { Id = "den-m", Sku = "AO-DEN-M", Price = 180000, Stock = 10, Options = new() { ["color"] = "Đen", ["size"] = "M" } },
            new() { Id = "trang-l", Sku = "AO-TRANG-L", Price = 190000, Stock = 8, Options = new() { ["color"] = "Trắng", ["size"] = "L" } }
        ]
    };

    [Fact]
    public void SelectedVariantControlsCartPriceStockAndSku()
    {
        var product = Shirt();
        var row = ProductSelection.CartRow(product, 2, "trang-l");
        Assert.Equal(380000, row.Total);
        Assert.Equal(8, row.Product.Stock);
        Assert.Equal("AO-TRANG-L", row.Product.Sku);
        Assert.Equal("trang-l", row.VariantId);
        Assert.Equal(0, product.Price);
    }

    [Fact]
    public void MissingOrInactiveVariantCannotBePurchased()
    {
        var product = Shirt();
        Assert.Throws<BusinessException>(() => ProductSelection.Variant(product, null));
        Assert.Throws<BusinessException>(() => ProductSelection.Variant(product, "invalid"));
        product.Variants[0].IsActive = false;
        Assert.Throws<BusinessException>(() => ProductSelection.Variant(product, "den-m"));
    }

    [Fact]
    public void SimpleProductKeepsItsPriceAndZeroStockInBson()
    {
        var product = new Product { Price = 240000, Stock = 0 };
        Assert.Equal(480000, ProductSelection.CartRow(product, 2, null).Total);
        Assert.True(product.ShouldSerializeStock());
        Assert.Throws<BusinessException>(() => ProductSelection.Variant(product, "invalid"));
    }

    [Fact]
    public void VariantProductOmitsRootPriceAndStockAndRetainsVariantId()
    {
        var product = Shirt();
        var bson = product.ToBsonDocument();
        Assert.DoesNotContain(bson.Names, key => key.Equals("price", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(bson.Names, key => key.Equals("stock", StringComparison.OrdinalIgnoreCase));
        var variants = bson.Elements.Single(e => e.Name.Equals("variants", StringComparison.OrdinalIgnoreCase)).Value.AsBsonArray;
        Assert.DoesNotContain("_id", variants[0].AsBsonDocument.Names);
        Assert.Contains(variants[0].AsBsonDocument.Names, key => key.Equals("id", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void InvoiceSnapshotKeepsOriginalVariantPriceAndOptions()
    {
        var product=Shirt();
        var snapshot=ProductSelection.Snapshot(product,new Category {Name="Thời trang"},2,"den-m");
        product.Name="Tên mới"; product.Variants[0].Price=999000; product.Variants[0].Options["color"]="Trắng";
        Assert.Equal(180000,snapshot.UnitPrice);
        Assert.Equal(360000,snapshot.LineTotal);
        Assert.Equal("Đen",snapshot.Variant!.Options["color"]);
        Assert.Equal("den-m",snapshot.Variant.Id);
        Assert.Equal("Áo thun",snapshot.Name);
    }
}
