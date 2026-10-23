using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using MongoDB.Bson;
using Xunit;

public class ProductCategoryValidationTests
{
    [Fact]
    public void ChildUsesRootEvenWhenRootIsRenamed()
    {
        var root = new Category { Name = "Tên mới" };
        var child = new Category { ParentId = root.Id };
        var leaf = new Category { ParentId = child.Id };
        Assert.Same(root, CategoryHierarchy.GetRoot(leaf.Id, new[] { root, child, leaf }));
    }

    [Fact]
    public void CyclesAreRejected()
    {
        var first = new Category();
        var second = new Category { ParentId = first.Id };
        first.ParentId = second.Id;
        Assert.Throws<BusinessException>(() => CategoryHierarchy.GetRoot(first.Id, new[] { first, second }));
    }

    [Fact]
    public void RootRulesValidateAttributeTypesAndRequiredFields()
    {
        var rules = new Dictionary<string, string> { ["brand"] = "string", ["warrantyMonths"] = "nonNegativeNumber" };
        MongoCatalogService.ValidateAttributes(rules, new BsonDocument { ["brand"] = "Demo", ["warrantyMonths"] = 12 });
        Assert.Throws<BusinessException>(() => MongoCatalogService.ValidateAttributes(rules, new BsonDocument { ["brand"] = "Demo" }));
        Assert.Throws<BusinessException>(() => MongoCatalogService.ValidateAttributes(rules, new BsonDocument { ["brand"] = "Demo", ["warrantyMonths"] = -1 }));
        Assert.Throws<BusinessException>(() => MongoCatalogService.ValidateAttributes(rules, new BsonDocument { ["brand"] = 123, ["warrantyMonths"] = 12 }));
    }
}
