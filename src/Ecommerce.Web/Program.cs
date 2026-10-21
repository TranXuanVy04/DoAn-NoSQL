using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

ConventionRegistry.Register("camelCase", new ConventionPack { new CamelCaseElementNameConvention() }, _ => true);
var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment()) {
    var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");
    if (File.Exists(envPath)) {
        // Read development settings without mutating process-wide environment variables.
        var values = DotNetEnv.Env.NoEnvVars().Load(envPath)
            .Select(pair => new KeyValuePair<string, string?>(pair.Key.Replace("__", ":"), pair.Value));
        builder.Configuration.AddInMemoryCollection(values);
        builder.Configuration.AddEnvironmentVariables();
        builder.Configuration.AddCommandLine(args);
    }
}
var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"];
builder.Services.AddCors(options => options.AddPolicy("Storefront", policy => policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
if (string.IsNullOrWhiteSpace(builder.Configuration["Mongo:ConnectionString"]) || string.IsNullOrWhiteSpace(builder.Configuration["Mongo:Database"]))
    throw new InvalidOperationException("Mongo configuration is required.");
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o => {
    o.IdleTimeout = TimeSpan.FromHours(2);
    o.Cookie.HttpOnly = true; o.Cookie.IsEssential = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o => { o.Events.OnRedirectToLogin = async context => {
        if (context.Request.Path.StartsWithSegments("/api")) {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { title = "Chưa đăng nhập", status = 401 }, options: (System.Text.Json.JsonSerializerOptions?)null, contentType: "application/problem+json");
        } else context.Response.Redirect(context.RedirectUri);
    };
    o.LoginPath = "/Account/Login"; o.ExpireTimeSpan = TimeSpan.FromHours(2); });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(builder.Configuration["Mongo:ConnectionString"]));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(builder.Configuration["Mongo:Database"] ?? "ecommerce_dev"));
builder.Services.AddScoped<ICatalogService, MongoCatalogService>();
builder.Services.AddScoped<IOrderService, MongoOrderService>();
builder.Services.AddScoped<IDashboardService, MongoDashboardService>();
var app = builder.Build();
app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), branch => {
    branch.UseExceptionHandler(handler => handler.Run(async context => {
        await Results.Problem(statusCode: 500, title: "Lỗi hệ thống").ExecuteAsync(context);
    }));
});
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"), branch => branch.UseExceptionHandler("/Home/Error"));
app.UseStaticFiles();
app.UseRouting();
app.UseCors("Storefront");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute("default", "{controller=Catalog}/{action=Index}/{id?}");
app.Run();
public partial class Program { }
