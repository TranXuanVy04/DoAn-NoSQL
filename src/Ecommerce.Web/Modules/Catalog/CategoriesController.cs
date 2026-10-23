using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Ecommerce.Web.Modules.Catalog;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICatalogService catalog;

    public CategoriesController(ICatalogService catalog)
    {
        this.catalog = catalog;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await catalog.Categories();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return Problem(statusCode: 400, title: "ID danh mục không hợp lệ.");

        var categories = await catalog.Categories();
        var category = categories.FirstOrDefault(x => x.Id == id);
        if (category == null)
            return Problem(statusCode: 404, title: "Không tìm thấy danh mục.");

        return Ok(category);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("csrf")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult GetCsrfToken([FromServices] IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new { token = tokens.RequestToken, headerName = "X-CSRF-TOKEN" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CategoryRequest request)
    {
        var input = new CategoryForm { Name = request.Name, ParentId = request.ParentId };
        try
        {
            var category = await catalog.SaveCategory(input);
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }
        catch (BusinessException ex)
        {
            return Problem(statusCode: 409, title: ex.Message);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return Problem(statusCode: 409, title: "Tên danh mục đã tồn tại.");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, CategoryRequest request)
    {
        if (!ObjectId.TryParse(id, out _))
            return Problem(statusCode: 400, title: "ID danh mục không hợp lệ.");

        var categories = await catalog.Categories();
        if (!categories.Any(x => x.Id == id))
            return Problem(statusCode: 404, title: "Không tìm thấy danh mục.");

        var input = new CategoryForm { Id = id, Name = request.Name, ParentId = request.ParentId };
        try
        {
            var category = await catalog.SaveCategory(input);
            return Ok(category);
        }
        catch (BusinessException ex)
        {
            return Problem(statusCode: 409, title: ex.Message);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return Problem(statusCode: 409, title: "Tên danh mục đã tồn tại.");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return Problem(statusCode: 400, title: "ID danh mục không hợp lệ.");

        var categories = await catalog.Categories();
        if (!categories.Any(x => x.Id == id))
            return Problem(statusCode: 404, title: "Không tìm thấy danh mục.");

        try
        {
            await catalog.DeleteCategory(id);
            return NoContent();
        }
        catch (BusinessException ex)
        {
            return Problem(statusCode: 409, title: ex.Message);
        }
    }
}
