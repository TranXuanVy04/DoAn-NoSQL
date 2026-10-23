using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
namespace Ecommerce.Web.Controllers;

[Authorize] public sealed class AdminController(ICatalogService catalog,IOrderService orders,IDashboardService dashboard) : Controller {
    [HttpGet] public async Task<IActionResult> Products(string? q,string? categoryId,int page=1) {
        ViewBag.Categories=await catalog.Categories(); ViewBag.Query=q; ViewBag.CategoryId=categoryId;
        try { return View(await catalog.Products(q,categoryId,page,true)); } catch(BusinessException ex) {return BadRequest(ex.Message);}
    }
    [HttpGet] public async Task<IActionResult> ProductEdit(string? id) {
        ViewBag.Categories=await catalog.Categories();
        if(id is null) return View(new ProductForm {AttributesJson="{\"brand\":\"Demo\",\"warrantyMonths\":12}"});
        var p=await catalog.Product(id); return p is null ? NotFound() : View(ProductForm.From(p));
    }
    [HttpPost] public async Task<IActionResult> ProductEdit(ProductForm f) {
        if(ModelState.IsValid) try {await catalog.SaveProduct(f); TempData["Message"]="Đã lưu sản phẩm.";return RedirectToAction(nameof(Products));}
        catch(BusinessException ex) {ModelState.AddModelError("",ex.Message);}
        catch(MongoWriteException ex) when(ex.WriteError.Category==ServerErrorCategory.DuplicateKey) {ModelState.AddModelError("","SKU đã tồn tại.");}
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
