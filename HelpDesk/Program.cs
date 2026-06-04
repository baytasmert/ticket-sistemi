using HelpDesk.Data;
using HelpDesk.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database konfigürasyonu
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=helpdesk.db";

// Azure App Service'te SQLite dosyası varsayılan olarak wwwroot içinde oluşur ve
// her deploy'da wwwroot baştan yazıldığı için silinir (tüm kullanıcı/talep verisi gider).
// Bu yüzden Azure'da DB'yi deploy'un dokunmadığı kalıcı klasöre (D:\home\data) taşıyoruz.
// WEBSITE_SITE_NAME yalnızca Azure App Service'te tanımlıdır; lokal geliştirme etkilenmez.
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME")))
{
    var dataDir = Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "/home", "data");
    Directory.CreateDirectory(dataDir);
    connectionString = $"Data Source={Path.Combine(dataDir, "helpdesk.db")}";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Identity konfigürasyonu
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cookie ve authentication ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Database migration ve seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    await SeedData.SeedRolesAndAdminAsync(scope.ServiceProvider);
    await SeedData.SeedCategoriesAsync(scope.ServiceProvider);
}

app.Run();

// Entegrasyon testlerinin (WebApplicationFactory) uygulamayı başlatabilmesi için
// Program sınıfını public partial olarak açığa çıkarıyoruz.
public partial class Program { }
