using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
namespace Ecommerce.Web.Modules.Catalog;

[ApiController, Route("api")]
public sealed class ProductsController(ICatalogService catalog) : ControllerBase
{
    public static object ProductDto(Ecommerce.Web.Models.Product p) => new {
        p.Id, p.Sku, p.Name, p.CategoryId, p.Kind, p.Price, p.Stock, p.IsActive,
        Attributes = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(p.Attributes.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { OutputMode = MongoDB.Bson.IO.JsonOutputMode.RelaxedExtendedJson }))
    };
    [HttpGet("products")]
    public async Task<IActionResult> Products(string? q, string? categoryId, int page = 1) {
        if (page < 1 || (categoryId is not null && !ObjectId.TryParse(categoryId, out _)))
            return Problem(statusCode:400, title:"Bộ lọc không hợp lệ");
        try {
            var result = await catalog.Products(q, categoryId, page);
            return Ok(new { Items = result.Items.Select(ProductDto), result.Total, result.Page, result.PageSize });
        } catch (Ecommerce.Web.Models.BusinessException ex) { return Problem(statusCode:400, title:ex.Message); }
    }

    [HttpGet("products/{id}")]
    public async Task<IActionResult> Product(string id) {
        if (!ObjectId.TryParse(id, out _)) return Problem(statusCode:400, title:"ID không hợp lệ");
        var product = await catalog.Product(id);
        return product is null || !product.IsActive ? Problem(statusCode:404, title:"Không tìm thấy") : Ok(ProductDto(product));
    }
}
