using HelpDesk.Data;
using HelpDesk.Models;
using HelpDesk.Services;
using Microsoft.AspNetCore.HttpOverrides;
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
// Şifre politikasının TEK yetkili yeri burası. Kullanıcı oluşturan her yol
// (Register, Admin → CreateUser, SeedData) bu kurallara tabi olur; kural
// ViewModel regex'leriyle tekrarlanmaz.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
})
.AddErrorDescriber<TurkishIdentityErrorDescriber>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cookie ve authentication ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Uygulama servisleri (iş mantığı katmanı).
// LoginThrottle paylaşılan durum tuttuğu için singleton; TicketService DbContext'e
// bağlı olduğu için request başına scoped'tır.
builder.Services.AddSingleton<LoginThrottle>();
builder.Services.AddScoped<ITicketService, TicketService>();

// Azure App Service gibi ters proxy arkasında istemcinin gerçek IP'sini (X-Forwarded-For)
// ve protokolünü (X-Forwarded-Proto) elde etmek için. Aksi halde tüm istekler proxy
// IP'sinden görünür ve IP bazlı giriş hız sınırlaması yanlış çalışır.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseForwardedHeaders();

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
