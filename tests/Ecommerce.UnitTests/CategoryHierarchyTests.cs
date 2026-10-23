using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Xunit;

public class CategoryHierarchyTests
{
    private const string Root = "000000000000000000000001";
    private const string Child = "000000000000000000000002";
    private const string Leaf = "000000000000000000000003";

    private static List<Category> Tree() => new()
    {
        new Category { Id = Root, Name = "Sách", ParentId = null },
        new Category { Id = Child, Name = "Sách lập trình", ParentId = Root },
        new Category { Id = Leaf, Name = "C#", ParentId = Child }
    };

    [Fact]
    public void RootNeedsNoParent()
    {
        CategoryHierarchy.ValidateParent(null, null, Tree());
    }

    [Fact]
    public void NewCategoryCanUseExistingParent()
    {
        CategoryHierarchy.ValidateParent(null, Child, Tree());
    }

    [Theory]
    [InlineData(Root, Root)]
    [InlineData(Root, Leaf)]
    [InlineData(Child, Leaf)]
    public void SelfOrDescendantParentIsRejected(string id, string parentId)
    {
        Assert.Throws<BusinessException>(() => CategoryHierarchy.ValidateParent(id, parentId, Tree()));
    }

    [Theory]
    [InlineData("bad-id")]
    [InlineData("000000000000000000000099")]
    public void MissingOrInvalidParentIsRejected(string parentId)
    {
        Assert.Throws<BusinessException>(() => CategoryHierarchy.ValidateParent(null, parentId, Tree()));
    }

    [Fact]
    public void ExistingCycleIsRejected()
    {
        var categories = Tree();
        categories[0].ParentId = Leaf;
        Assert.Throws<BusinessException>(() => CategoryHierarchy.ValidateParent(null, Root, categories));
    }

    [Fact]
    public void ParentIdIsStoredAsObjectIdAndRootAsNull()
    {
        var child = new Category { Id = Child, ParentId = Root };
        var bson = child.ToBsonDocument();
        // Explicit member name avoids depending on whether Program registered camelCase yet.
        var parentField = bson.Elements.Single(x => x.Name.Equals("parentId", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(BsonType.ObjectId, parentField.Value.BsonType);
        Assert.Equal(Root, BsonSerializer.Deserialize<Category>(bson).ParentId);
        child.ParentId = null;
        Assert.True(child.ToBsonDocument()[parentField.Name].IsBsonNull);
    }

    [Fact]
    public void LegacyCategoryCanStillBeReadBeforeMigration()
    {
        // BsonIgnoreExtraElements allows old category.kind to be read without retaining it.
        var bson = new Category { Id = Root, Name = "Điện tử" }.ToBsonDocument();
        bson["kind"] = "electronics";
        var category = BsonSerializer.Deserialize<Category>(bson);
        Assert.Equal("Điện tử", category.Name);
        Assert.Null(category.ParentId);
    }
}
