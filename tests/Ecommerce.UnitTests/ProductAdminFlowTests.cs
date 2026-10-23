using System.Net;
using System.Text.RegularExpressions;
using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

public class ProductAdminFlowTests
{
    private static async Task<string> Token(HttpClient client,string path)
    {
        string html=await client.GetStringAsync(path);
        var match=Regex.Match(html,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success);
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    [Fact]
    public async Task VariantFormCanCreateEditDeactivateAndRejectStaleWrites()
    {
        using var factory=new ProductAdminFactory();
        using var client=factory.CreateClient(new() {BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});
        client.DefaultRequestHeaders.Add("X-Test-Admin","yes");
        string token=await Token(client,"/Admin/ProductEdit");
        var form=new Dictionary<string,string> {
            ["Sku"]="SHIRT",["Name"]="Áo cotton",["CategoryId"]=ProductMemoryCatalog.CategoryId,
            ["HasVariants"]="true",["IsActive"]="true",["AttributesJson"]="{\"material\":\"Cotton\"}",
            ["Variants.Index"]="newrow",["Variants[newrow].Sku"]="SHIRT-DEN-M",["Variants[newrow].Color"]="Đen",
            ["Variants[newrow].Size"]="M",["Variants[newrow].Price"]="180000",["Variants[newrow].Stock"]="10",
            ["Variants[newrow].OptionsJson"]="{}",["Variants[newrow].IsActive"]="true",["__RequestVerificationToken"]=token
        };
        Assert.Equal(HttpStatusCode.Redirect,(await client.PostAsync("/Admin/ProductEdit",new FormUrlEncodedContent(form))).StatusCode);
        var catalog=factory.Services.GetRequiredService<ICatalogService>();
        var product=(await catalog.Products(null,null,1,true)).Items.Single();
        Assert.Single(product.Variants);
        string edit=await client.GetStringAsync("/Admin/ProductEdit/"+product.Id);
        Assert.Contains("/js/product-editor.js",edit);
        Assert.Contains("SHIRT-DEN-M",edit);
        form["Id"]=product.Id;
        form["OriginalRevision"]=product.Revision.ToString();
        form["Variants[newrow].Id"]=product.Variants[0].Id;
        form["Variants[newrow].Price"]="195000";
        form["Variants[newrow].Stock"]="7";
        form["__RequestVerificationToken"]=await Token(client,"/Admin/ProductEdit/"+product.Id);
        Assert.Equal(HttpStatusCode.Redirect,(await client.PostAsync("/Admin/ProductEdit",new FormUrlEncodedContent(form))).StatusCode);
        var saved=(await catalog.Product(product.Id))!;
        Assert.Equal(195000,saved.Variants[0].Price);
        Assert.Equal(7,saved.Variants[0].Stock);
        var stale=await client.PostAsync("/Admin/ProductEdit",new FormUrlEncodedContent(form));
        Assert.Equal(HttpStatusCode.OK,stale.StatusCode);
        Assert.Contains("vừa thay đổi",WebUtility.HtmlDecode(await stale.Content.ReadAsStringAsync()));
        var delete=new Dictionary<string,string> { ["id"]=product.Id,["__RequestVerificationToken"]=form["__RequestVerificationToken"] };
        Assert.Equal(HttpStatusCode.Redirect,(await client.PostAsync("/Admin/ProductDelete",new FormUrlEncodedContent(delete))).StatusCode);
        Assert.False((await catalog.Product(product.Id))!.IsActive);
    }
    [Fact]
    public async Task TwoVariantsStaySeparateAndRemovingOneKeepsTheOther()
    {
        using var factory=new ProductAdminFactory();
        using var client=factory.CreateClient(new() {BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});
        client.DefaultRequestHeaders.Add("X-Test-Admin","yes");
        var catalog=factory.Services.GetRequiredService<ICatalogService>();
        await catalog.SaveProduct(new ProductForm {
            Name="Áo cotton",Sku="AO",CategoryId=ProductMemoryCatalog.CategoryId,HasVariants=true,AttributesJson="{\"material\":\"Cotton\"}",
            Variants=[new() {Sku="AO-DEN-M",Color="Đen",Size="M",Price=180000,Stock=10},new() {Sku="AO-TRANG-L",Color="Trắng",Size="L",Price=190000,Stock=8}]
        });
        var product=(await catalog.Products(null,null,1,true)).Items.Single();
        var token=await Token(client,"/Admin/ProductEdit");
        async Task<HttpResponseMessage> Post(string action,string variant,int quantity) => await client.PostAsync(action,new FormUrlEncodedContent(new Dictionary<string,string> {
            ["productId"]=product.Id,["variantId"]=variant,["quantity"]=quantity.ToString(),["__RequestVerificationToken"]=token
        }));
        foreach(var variant in product.Variants) Assert.Equal(HttpStatusCode.Redirect,(await Post("/Cart/Add",variant.Id,2)).StatusCode);
        string cart=WebUtility.HtmlDecode(await client.GetStringAsync("/Cart"));
        Assert.Contains("AO-DEN-M",cart); Assert.Contains("AO-TRANG-L",cart);
        var checkout=WebUtility.HtmlDecode(await client.GetStringAsync("/Cart/Checkout"));
        Assert.Contains("Đen",checkout); Assert.Contains("Trắng",checkout);
        Assert.Equal(HttpStatusCode.Redirect,(await Post("/Cart/Update",product.Variants[0].Id,0)).StatusCode);
        cart=WebUtility.HtmlDecode(await client.GetStringAsync("/Cart"));
        Assert.DoesNotContain("AO-DEN-M",cart); Assert.Contains("AO-TRANG-L",cart);
        Assert.Equal(HttpStatusCode.Redirect,(await Post("/Cart/Add",product.Variants[1].Id,99)).StatusCode);
        Assert.Contains("vượt tồn kho",WebUtility.HtmlDecode(await client.GetStringAsync("/Cart")));
    }

}

public class ProductAdminFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Mongo:ConnectionString","mongodb://localhost:27017");
        builder.UseSetting("Mongo:Database","ecommerce_test");
        builder.ConfigureServices(services=> {
            services.RemoveAll<ICatalogService>();
            services.AddSingleton<ICatalogService,ProductMemoryCatalog>();
            services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions,CategoryTestAuth>("Test",_=>{});
        });
    }
}

public class ProductMemoryCatalog : ICatalogService
{
    public const string CategoryId="000000000000000000000002";
    private readonly List<Product> rows=[];
    public Task<List<Category>> Categories()=>Task.FromResult(new List<Category> {new() {Id=CategoryId,Name="Thời trang"}});
    public Task<ProductPage> Products(string? query,string? categoryId,int page,bool admin=false)=>Task.FromResult(new ProductPage(rows.Where(p=>admin||p.IsActive).ToList(),rows.Count,page,12));
    public Task<Product?> Product(string id)=>Task.FromResult(rows.FirstOrDefault(p=>p.Id==id));
    public Task SaveProduct(ProductForm form)
    {
        var existing=rows.FirstOrDefault(p=>p.Id==form.Id);
        if(existing!=null && existing.Revision!=form.OriginalRevision) throw new BusinessException("Sản phẩm vừa thay đổi.");
        var product=ProductFormMapper.Build(form,existing,new Dictionary<string,string> {["material"]="string",["size"]="string"});
        rows.RemoveAll(p=>p.Id==product.Id); rows.Add(product); return Task.CompletedTask;
    }
    public Task DeleteProduct(string id) {rows.Single(p=>p.Id==id).IsActive=false;return Task.CompletedTask;}
    public Task<Category> SaveCategory(CategoryForm input)=>throw new NotSupportedException();
    public Task DeleteCategory(string id)=>throw new NotSupportedException();
}
