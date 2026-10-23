using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using MongoDB.Bson;
using Xunit;

public class ProductFormMapperTests
{
    private static readonly Dictionary<string,string> Rules = new() { ["material"]="string", ["size"]="string" };
    private static ProductForm Shirt() => new() {
        Name="Áo cotton", Sku="AO-COTTON", CategoryId="000000000000000000000002", HasVariants=true,
        AttributesJson="{\"material\":\"Cotton\"}",
        Variants=[new() {Sku="AO-DEN-M",Color="Đen",Size="M",Price=180000,Stock=10},new() {Sku="AO-TRANG-L",Color="Trắng",Size="L",Price=190000,Stock=8}]
    };

    [Fact]
    public void BuildsVariantsAndValidatesSizeFromOptions()
    {
        var p=ProductFormMapper.Build(Shirt(),null,Rules);
        Assert.Equal(2,p.Variants.Count);
        Assert.NotEqual(p.Variants[0].Id,p.Variants[1].Id);
        Assert.Equal(0,p.Price);
        Assert.Equal(0,p.Stock);
        Assert.Equal("M",p.Variants[0].Options["size"]);
        var edit=ProductForm.From(p);
        Assert.Equal(p.Variants[0].Id,edit.Variants[0].Id);
        var saved=ProductFormMapper.Build(edit,p,Rules);
        Assert.Equal(p.Revision+1,saved.Revision);
        Assert.Equal(p.Variants[0].Id,saved.Variants[0].Id);
    }

    [Fact]
    public void DuplicateSkuOrCombinationIsRejected()
    {
        var f=Shirt(); f.Variants[1].Sku=f.Variants[0].Sku;
        Assert.Throws<BusinessException>(()=>ProductFormMapper.Build(f,null,Rules));
        f=Shirt(); f.Variants[1].Color="Đen"; f.Variants[1].Size="M";
        Assert.Throws<BusinessException>(()=>ProductFormMapper.Build(f,null,Rules));
    }

    [Fact]
    public void MissingSizeAndNegativeStockAreRejected()
    {
        var f=Shirt(); f.Variants[0].Size=null;
        Assert.Throws<BusinessException>(()=>ProductFormMapper.Build(f,null,Rules));
        f=Shirt(); f.Variants[0].Stock=-1;
        Assert.Throws<BusinessException>(()=>ProductFormMapper.Build(f,null,Rules));
    }

    [Fact]
    public void ExistingVariantCannotBeRemovedButCanBeDeactivated()
    {
        var p=ProductFormMapper.Build(Shirt(),null,Rules);
        var f=ProductForm.From(p); f.Variants.RemoveAt(0);
        Assert.Throws<BusinessException>(()=>ProductFormMapper.Build(f,p,Rules));
        f=ProductForm.From(p); f.Variants[0].IsActive=false;
        Assert.False(ProductFormMapper.Build(f,p,Rules).Variants[0].IsActive);
    }

    [Fact]
    public void SimpleProductKeepsZeroStockAndCannotChangeTypeAfterCreation()
    {
        var f=new ProductForm {Name="Bình nước",Sku="BINH",CategoryId="000000000000000000000003",Price=240000,Stock=0};
        var p=ProductFormMapper.Build(f,null,new Dictionary<string,string>());
        Assert.Empty(p.Variants);
        Assert.True(p.ShouldSerializeStock());
        f.HasVariants=true;
        Assert.Throws<BusinessException>(()=>ProductFormMapper.Build(f,p,new Dictionary<string,string>()));
    }
}
