using Ecommerce.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace Ecommerce.Web.Controllers;

public sealed class AccountController(IConfiguration config) : Controller {
    [HttpGet] public IActionResult Login() => View(new LoginForm());
    [HttpPost] public async Task<IActionResult> Login(LoginForm f,string? returnUrl) {
        var expected=config["Admin:Password"]??"";
        var ok=ModelState.IsValid && expected.Length>=12 && f.Username==config["Admin:Username"] &&
            CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(f.Password)),SHA256.HashData(Encoding.UTF8.GetBytes(expected)));
        if(!ok){ModelState.AddModelError("","Sai tài khoản/mật khẩu hoặc chưa cấu hình mật khẩu Admin tối thiểu 12 ký tự.");return View(f);}
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name,f.Username),new Claim(ClaimTypes.Role,"Admin")],CookieAuthenticationDefaults.AuthenticationScheme)));
        return LocalRedirect(Url.IsLocalUrl(returnUrl)?returnUrl!:"/Admin/Dashboard");
    }
    [HttpPost] public async Task<IActionResult> Logout(){await HttpContext.SignOutAsync();return RedirectToAction("Index","Catalog");}
}
