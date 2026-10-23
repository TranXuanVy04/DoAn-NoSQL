using Ecommerce.Web.Repositories;
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
if (string.IsNullOrWhiteSpace(builder.Configuration["Mongo:ConnectionString"]) || string.IsNullOrWhiteSpace(builder.Configuration["Mongo:Database"]))
    throw new InvalidOperationException("Mongo configuration is required.");
builder.Services.AddAntiforgery();
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o => o.ValueCountLimit = 4096);
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o => {
    o.IdleTimeout = TimeSpan.FromHours(2);
    o.Cookie.HttpOnly = true; o.Cookie.IsEssential = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o => {
        o.LoginPath = "/Account/Login";
        o.ExpireTimeSpan = TimeSpan.FromHours(2);
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(builder.Configuration["Mongo:ConnectionString"]));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(builder.Configuration["Mongo:Database"] ?? "ecommerce_dev"));
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<AnalyticsRepository>();
if (builder.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("Storefront:UseDemoData"))
    builder.Services.AddScoped<ICatalogService, DemoCatalogService>();
else
    builder.Services.AddScoped<ICatalogService, MongoCatalogService>();
builder.Services.AddScoped<IOrderService, MongoOrderService>();
builder.Services.AddScoped<IDashboardService, MongoDashboardService>();
var app = builder.Build();
app.UseExceptionHandler("/Home/Error");
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Catalog}/{action=Index}/{id?}");
app.Run();
public partial class Program { }
