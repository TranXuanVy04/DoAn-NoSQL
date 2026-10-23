using System.Text.Json;
using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Ecommerce.Web.ViewComponents;

public class StorefrontHeaderViewComponent : ViewComponent
{
    private readonly ICatalogService catalog;
    private readonly ILogger<StorefrontHeaderViewComponent> logger;

    public StorefrontHeaderViewComponent(ICatalogService catalog, ILogger<StorefrontHeaderViewComponent> logger)
    {
        this.catalog = catalog;
        this.logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var model = new StorefrontHeaderViewModel();
        try { model.Categories = await catalog.Categories(); }
        catch (MongoException) { logger.LogWarning("Catalog unavailable while rendering storefront header."); }
        if (model.Categories.Count == 0) model.Categories = StorefrontDemo.Categories;
        try {
            var cart = JsonSerializer.Deserialize<List<CartLine>>(HttpContext.Session.GetString("Cart") ?? "[]");
            model.CartQuantity = cart?.Sum(x => x.Quantity) ?? 0;
        } catch (JsonException) { model.CartQuantity = 0; }
        return View(model);
    }
}
