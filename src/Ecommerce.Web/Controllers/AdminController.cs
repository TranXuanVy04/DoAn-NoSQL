using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
namespace Ecommerce.Web.Controllers;

[Authorize(Roles = "Admin")] public sealed class AdminController(ICatalogService catalog,IOrderService orders,IDashboardService dashboard) : Controller {
    [HttpGet] public async Task<IActionResult> Products(string? q,string? categoryId,int page=1) {
        ViewBag.Categories=await catalog.Categories(); ViewBag.Query=q; ViewBag.CategoryId=categoryId;
        try { return View(await catalog.Products(q,categoryId,page,true)); } catch(BusinessException ex) {return BadRequest(ex.Message);}
    }
    [HttpGet] public async Task<IActionResult> ProductEdit(string? id) {
        ViewBag.Categories=await catalog.Categories();
        if(id is null) return View(new ProductForm());
        var p=await catalog.Product(id); return p is null ? NotFound() : View(ProductForm.From(p));
    }
    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)] public async Task<IActionResult> ProductEdit(ProductForm f, IFormFile? imageFile = null) {
        string? uploadedPath = null;
        string? originalImage = f.ImageUrl;
        if(ModelState.IsValid) try {
            if (imageFile is { Length: > 0 }) {
                if (imageFile.Length > 5 * 1024 * 1024) throw new BusinessException("Ảnh tối đa 5 MB.");
                await using var stream = imageFile.OpenReadStream();
                var header = new byte[12];
                int count = await stream.ReadAtLeastAsync(header,12,throwOnEndOfStream:false);
                string? extension = count >= 8 && header.AsSpan(0,8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10}) ? ".png"
                    : count >= 3 && header[0] == 255 && header[1] == 216 && header[2] == 255 ? ".jpg"
                    : count >= 12 && System.Text.Encoding.ASCII.GetString(header,0,4) == "RIFF" && System.Text.Encoding.ASCII.GetString(header,8,4) == "WEBP" ? ".webp" : null;
                if (extension == null) throw new BusinessException("Chọn ảnh JPG, PNG hoặc WebP hợp lệ.");
                var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
                var directory = Path.Combine(env.WebRootPath,"uploads","products");
                Directory.CreateDirectory(directory);
                string filename = Guid.NewGuid().ToString("N") + extension;
                uploadedPath = Path.Combine(directory,filename);
                await using (var output = System.IO.File.Create(uploadedPath)) {
                    await output.WriteAsync(header.AsMemory(0,count));
                    await stream.CopyToAsync(output);
                }
                f.ImageUrl = "/uploads/products/" + filename;
            }
            await catalog.SaveProduct(f); TempData["Message"]="Đã lưu sản phẩm.";return RedirectToAction(nameof(Products));}
        catch(BusinessException ex) {ModelState.AddModelError("",ex.Message);}
        catch(MongoWriteException ex) when(ex.WriteError.Category==ServerErrorCategory.DuplicateKey) {ModelState.AddModelError("","SKU đã tồn tại.");}
        if (uploadedPath != null) { System.IO.File.Delete(uploadedPath); f.ImageUrl = originalImage; }
        ViewBag.Categories=await catalog.Categories();return View(f);
    }
    [HttpPost] public async Task<IActionResult> ProductDelete(string id) {
        try {await catalog.DeleteProduct(id);TempData["Message"]="Đã ngừng bán sản phẩm.";}catch(BusinessException ex){TempData["Error"]=ex.Message;}
        return RedirectToAction(nameof(Products));
    }
    [HttpGet] public async Task<IActionResult> Categories() => View(await catalog.Categories());
    [HttpGet] public async Task<IActionResult> CategoryEdit(string? id) {
        ViewBag.Categories = await catalog.Categories();
        if(id is null)return View(new CategoryForm());
        var c=(await catalog.Categories()).FirstOrDefault(x=>x.Id==id);
        return c is null ? NotFound() : View(new CategoryForm{Id=c.Id,Name=c.Name,ParentId=c.ParentId});
    }
    [HttpPost] public async Task<IActionResult> CategoryEdit(CategoryForm f) {
        ViewBag.Categories = await catalog.Categories();
        if(ModelState.IsValid) try {await catalog.SaveCategory(f);return RedirectToAction(nameof(Categories));}
        catch(BusinessException ex){ModelState.AddModelError("",ex.Message);}
        catch(MongoWriteException ex) when(ex.WriteError.Category==ServerErrorCategory.DuplicateKey){ModelState.AddModelError("","Tên danh mục trùng.");}
        return View(f);
    }
    [HttpPost] public async Task<IActionResult> CategoryDelete(string id) {
        try{await catalog.DeleteCategory(id);}catch(BusinessException ex){TempData["Error"]=ex.Message;}
        return RedirectToAction(nameof(Categories));
    }
    [HttpGet] public async Task<IActionResult> Orders(int page=1,string? status=null) {ViewBag.Status=status;return View(await orders.List(page,status));}
    [HttpPost] public async Task<IActionResult> OrderStatus(string id,string status) {
        try{await orders.ChangeStatus(id,status);TempData["Message"]="Đã cập nhật trạng thái.";}catch(BusinessException ex){TempData["Error"]=ex.Message;}
        return RedirectToAction("Details","Orders",new{id});
    }
    [HttpGet] public async Task<IActionResult> Dashboard(DateOnly? from,DateOnly? to,string group="day") {
        if(!ModelState.IsValid)return BadRequest("Ngày không hợp lệ; dùng yyyy-MM-dd.");
        var today=DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        try{return View(await dashboard.Report(from??today.AddDays(-90),to??today,group));}catch(BusinessException ex){return BadRequest(ex.Message);}
    }
}
