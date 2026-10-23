using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
namespace Ecommerce.Web.Controllers;

public sealed class CatalogController(ICatalogService catalog) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? q, string? categoryId, int page = 1)
    {
        ViewBag.Categories = await catalog.Categories(); 
        ViewBag.Query = q; ViewBag.CategoryId = categoryId;
        try { return View("Index", await catalog.Products(q, categoryId, page)); }
        catch (BusinessException ex) { return BadRequest(ex.Message); }
    }
    [HttpGet]
    public Task<IActionResult> Shop(string? q, string? categoryId, int page = 1)
    {
        ViewBag.IsShop = true;
        return Index(q, categoryId, page);
    }
    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        if (id.StartsWith("demo-"))
        {
            var demo = StorefrontDemo.Products.FirstOrDefault(x => x.Id == id);
            return demo == null ? NotFound() : View(demo);
        }
        var p = await catalog.Product(id); return p is null || !p.IsActive ? NotFound() : View(p);
    }
}
public sealed class CartController(ICatalogService catalog, IOrderService orders) : Controller
{
    private List<CartLine> Read() => JsonSerializer.Deserialize<List<CartLine>>(HttpContext.Session.GetString("Cart") ?? "[]") ?? [];
    private void Save(List<CartLine> rows) => HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(rows));
    private string Owner()
    {
        var token = HttpContext.Session.GetString("Owner");
        if (token is null) { token = Guid.NewGuid().ToString("N"); HttpContext.Session.SetString("Owner", token); }
        return token;
    }
    private async Task<CartView> Summary()
    {
        var rows = new List<CartRow>();
        foreach (var line in Read()) { var p = await catalog.Product(line.ProductId); if (p is not null) { try { rows.Add(ProductSelection.CartRow(p, line.Quantity, line.VariantId)); } catch (BusinessException) { rows.Add(new CartRow(new Product { Id = p.Id, Name = p.Name + " (biến thể ngừng bán)", ImageUrl = p.ImageUrl, IsActive = false }, line.Quantity, line.VariantId)); } } }
        return new(rows);
    }
    [HttpGet] public async Task<IActionResult> Index() => View(await Summary());
    [HttpPost]
    public async Task<IActionResult> Add(string productId, int quantity = 1, string? variantId = null)
    {
        if (string.IsNullOrWhiteSpace(productId)) return BadRequest("Mã sản phẩm không hợp lệ.");
        if (productId.StartsWith("demo-")) return BadRequest("Sản phẩm minh họa chưa mở bán.");
        if (!ModelState.IsValid || quantity is < 1 or > 99) return BadRequest("Số lượng phải từ 1 đến 99.");
        var p = await catalog.Product(productId); if (p is null || !p.IsActive) return NotFound();
        CartRow selected;
        try { selected = ProductSelection.CartRow(p, quantity, variantId); }
        catch (BusinessException ex) { TempData["Error"] = ex.Message; return RedirectToAction("Details", "Catalog", new { id = productId }); }
        var rows = Read(); var old = rows.FirstOrDefault(x => x.ProductId == productId && x.VariantId == variantId); var count = (old?.Quantity ?? 0) + quantity;
        if (count > 99 || count > selected.Product.Stock || (old is null && rows.Count >= 50)) TempData["Error"] = "Số lượng vượt tồn kho hoặc giới hạn giỏ hàng.";
        else { rows.RemoveAll(x => x.ProductId == productId && x.VariantId == variantId); rows.Add(new(productId, count, variantId)); Save(rows); TempData["Message"] = "Đã thêm vào giỏ hàng."; }
        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    public async Task<IActionResult> Update(string productId, int quantity, string? variantId = null)
    {
        if (!ModelState.IsValid || quantity is < 0 or > 99) return BadRequest("Số lượng không hợp lệ.");
        var rows = Read();
        if (quantity > 0)
        {
            var p = await catalog.Product(productId);
            CartRow? selected = null;
            if (p != null) try { selected = ProductSelection.CartRow(p, quantity, variantId); } catch (BusinessException) { }
            if (selected is null || !selected.Product.IsActive || selected.Product.Stock < quantity) { TempData["Error"] = "Sản phẩm ngừng bán hoặc không đủ tồn kho."; return RedirectToAction(nameof(Index)); }
        }
        if (rows.Any(x => x.ProductId == productId && x.VariantId == variantId)) { rows.RemoveAll(x => x.ProductId == productId && x.VariantId == variantId); if (quantity > 0) rows.Add(new(productId, quantity, variantId)); Save(rows); }
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        if (Read().Count == 0) return RedirectToAction(nameof(Index));
        Owner();
        var key = HttpContext.Session.GetString("CheckoutKey") ?? Guid.NewGuid().ToString("N");
        HttpContext.Session.SetString("CheckoutKey", key);
        ViewBag.Cart = await Summary(); return View(new CheckoutForm { CheckoutKey = key });
    }
    [HttpPost]
    public async Task<IActionResult> Checkout(CheckoutForm f)
    {
        var token = Owner(); var active = HttpContext.Session.GetString("CheckoutKey"); var previous = HttpContext.Session.GetString("LastCheckoutKey");
        if (string.IsNullOrEmpty(f.CheckoutKey) || (f.CheckoutKey != active && f.CheckoutKey != previous)) return BadRequest("Phiên đặt hàng hết hạn. Hãy mở lại trang thanh toán.");
        var existing = await orders.FindCheckout(f.CheckoutKey, token);
        if (existing is not null) { TempData["OrderNumber"] = existing.Number; return RedirectToAction(nameof(Success)); }
        if (ModelState.IsValid) try
            {
                var order = await orders.Place(Read(), new Customer { Name = f.Name.Trim(), Phone = f.Phone.Trim(), Address = f.Address.Trim() }, f.CheckoutKey, token);
                Save([]); HttpContext.Session.SetString("LastCheckoutKey", f.CheckoutKey); HttpContext.Session.Remove("CheckoutKey");
                TempData["OrderNumber"] = order.Number;
                return RedirectToAction(nameof(Success));
            }
            catch (BusinessException ex) { ModelState.AddModelError("", ex.Message); }
        ViewBag.Cart = await Summary(); return View(f);
    }
    [HttpGet]
    public IActionResult Success()
    {
        var number=TempData["OrderNumber"] as string;
        return string.IsNullOrEmpty(number) ? RedirectToAction("Index","Catalog") : View(model:number);
    }
}
public sealed class OrdersController(IOrderService orders) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int page=1)
    {
        var token=HttpContext.Session.GetString("Owner");
        return View(token == null ? new OrderPage([],0,1,15) : await orders.ListForCustomer(token,page));
    }
    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var o = await orders.Get(id);
        if (o is null || (!User.IsInRole("Admin") && o.CustomerToken != HttpContext.Session.GetString("Owner"))) return NotFound();
        return View(o);
    }
}
public sealed class HomeController : Controller
{
    [HttpGet, HttpPost][IgnoreAntiforgeryToken] public IActionResult Error() { Response.StatusCode = 500; return View(); }
}
