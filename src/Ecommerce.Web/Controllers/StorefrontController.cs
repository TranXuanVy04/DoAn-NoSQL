using Ecommerce.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Web.Controllers;

public class StorefrontController : Controller
{
    public IActionResult Blog() => View();
    public IActionResult Article(int id)
    {
        var article = StorefrontDemo.Articles.FirstOrDefault(x => x.Id == id);
        return article == null ? NotFound() : View(article);
    }
    public IActionResult Contact() => View();
}
