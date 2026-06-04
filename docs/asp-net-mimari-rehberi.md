# 🏛️ HelpDesk — ASP.NET Core Mimari Rehberi (CS öğrencisi seviyesi)

> Hedef kitle: genel programlama/CS bilen ama **ASP.NET Core'a yeni** biri.
> Temel kavramları (class, HTTP, SQL, DI'nin ne olduğu) bildiğini varsayıyorum; **framework'e özgü** mekaniğe odaklanıyorum.
> Her bölüm: **📖 ASP.NET Sözlüğü → 🔧 Mekanik → 🎓 Hocaya neyi anlatmalıyım** sırasıyla.

---

# 🗺️ GENEL MİMARİ — ASP.NET Core Bir İsteği Nasıl İşler?

ASP.NET Core, **self-hosted** bir uygulamadır (Kestrel web sunucusu uygulamanın içindedir; IIS/Azure önünde reverse-proxy olarak durur). Çalışma modeli:

```
HTTP İstek
  │
  ▼  Kestrel (gömülü web sunucusu)
  ▼  Middleware Pipeline  →  UseHttpsRedirection → UseRouting
  │                          → UseAuthentication → UseAuthorization
  ▼  Endpoint Routing  →  hangi Controller.Action?
  ▼  MVC katmanı:
        Model Binding  →  Action params/ViewModel'e doldur
        Validation     →  DataAnnotations → ModelState
        Action çalışır →  (DI ile DbContext, UserManager enjekte)
        IActionResult  →  View() / RedirectToAction() / NotFound()
  ▼  Razor View Engine  →  .cshtml'i HTML'e derle/render et
  │
  ▼  HTTP Cevap (HTML)
```

**Bu projenin katman haritası (MVC + EF Core):**
```
Controllers/   → HTTP isteklerini karşılar (ince; iş mantığı + DB erişimi burada)
ViewModels/    → Request/response için DTO; DataAnnotations ile validation
Models/        → EF Core entity'leri (domain + DB tabloları)
Data/          → DbContext (Unit of Work), Migrations, Seed
Views/         → Razor (.cshtml) — server-side rendering
Program.cs     → Composition root: DI kaydı + middleware pipeline
```

> **Mimari not:** Bu projede ayrı bir Service/Repository katmanı yok; controller'lar DbContext'i doğrudan kullanıyor. Küçük projeler için kabul edilebilir; büyürse Service katmanı + Repository pattern eklenir. (Hocaya bu trade-off'u söylemek olgunluk gösterir.)

---

# 📘 BÖLÜM 1 — Uygulama Başlatma: DI Container & Middleware Pipeline

### 📖 ASP.NET Sözlüğü
| Terim | ASP.NET'te ne demek |
|---|---|
| **Minimal Hosting Model** | .NET 6+ ile `Startup.cs` kalktı; her şey `Program.cs`'te top-level statements ile. |
| **WebApplicationBuilder** | `builder` — servisleri (DI) ve config'i kuran nesne. |
| **DI Container (IServiceCollection)** | `builder.Services` — bağımlılıkların kaydedildiği yerleşik IoC container. |
| **Service Lifetime** | Kayıt ömrü: **Singleton** (tek), **Scoped** (istek başına bir), **Transient** (her çağrıda yeni). |
| **Middleware** | Pipeline'da isteği işleyen `RequestDelegate` zinciri; `app.Use...` ile eklenir. **Sıra önemlidir.** |
| **Configuration** | `appsettings.json` + environment variables + user secrets'ı katmanlı birleştiren sistem (`IConfiguration`). |
| **Composition Root** | Tüm bağımlılıkların tek yerde bağlandığı nokta = `Program.cs`. |

### 🔧 Mekanik
- `AddDbContext<ApplicationDbContext>(UseSqlite(...))` → `DbContext`'i **Scoped** olarak kaydeder. Yani **HTTP isteği başına bir DbContext** instance'ı; istek bitince dispose edilir. (Change tracker'ın istekler arası sızmaması için kritik.)
- `AddIdentity<ApplicationUser, IdentityRole>()` → `UserManager`, `SignInManager`, `RoleManager`'ı (hepsi Scoped) ve cookie auth handler'larını DI'a ekler. `AddEntityFrameworkStores` → Identity'nin user/role store'unu EF Core'a bağlar.
- **Pipeline sırası neden önemli:** `UseAuthentication()` **`UseAuthorization()`'dan önce** olmalı — önce `HttpContext.User` (ClaimsPrincipal) doldurulur (authN), sonra `[Authorize]` onu değerlendirir (authZ). `UseRouting` da auth'tan önce gelir ki endpoint metadata (hangi action, hangi `[Authorize]`) bilinsin.
- **Otomatik migration:** `app.Services.CreateScope()` ile bir scope açıp `db.Database.Migrate()` çağırıyorum. Neden scope? DbContext Scoped olduğu için root provider'dan direkt çözülemez; manuel scope gerekir.

### 🎓 Hocaya neyi anlatmalıyım
1. **"Startup yerine minimal hosting kullandım"** — neden: .NET 6+ standardı.
2. **DI lifetime'larını biliyorum:** "DbContext'i Scoped kaydettim çünkü change tracker istek başına izole olmalı; Singleton yapsaydım thread-safety ve stale data sorunları olurdu."
3. **Middleware sırasının anlamı:** "AuthN, AuthZ'den önce gelmek zorunda; yoksa `[Authorize]` boş bir User görür."
4. **Config katmanlaması:** "Connection string `appsettings.json`'da ama Azure'da environment variable ile override edilebiliyor — kod değişmeden ortam değişiyor."

---

# 📗 BÖLÜM 2 — Authentication & Authorization (ASP.NET Core Identity)

### 📖 ASP.NET Sözlüğü
| Terim | ASP.NET'te ne demek |
|---|---|
| **ASP.NET Core Identity** | Membership framework; user/role store, password hashing, sign-in mantığı hazır gelir. |
| **UserManager / SignInManager / RoleManager** | Sırasıyla: user CRUD & rol, sign-in/out & şifre doğrulama, rol yönetimi. |
| **Authentication Scheme** | Kimliğin nasıl taşındığı. Burada **cookie-based** (`Identity.Application` scheme). |
| **ClaimsPrincipal (`HttpContext.User`)** | Giriş yapan kullanıcının kimlik+claim'lerini taşıyan nesne. |
| **Claim** | Kullanıcı hakkında bir bilgi parçası (rol, id, email…). Cookie içinde şifreli taşınır. |
| **Authorization Filter (`[Authorize]`)** | Action/Controller'a erişimi rol/policy ile sınırlayan MVC filter'ı. |
| **Antiforgery (`[ValidateAntiForgeryToken]`)** | Synchronizer-token pattern: cookie + hidden form alanı eşleşmesi (CSRF koruması). |
| **Password Hasher** | Identity'nin varsayılanı: **PBKDF2 (HMAC-SHA256), per-user salt**, çok iterasyon. |

### 🔧 Mekanik
- **İki ayrı giriş kapısı** ama **tek Identity şeması**: `AccountController` (Customer) ve `StaffController` (`[Route("staff")]` → `/staff/login`). Ayrım **role kontrolü** ile yapılıyor; ayrı cookie scheme değil.
- **Register:** `userManager.CreateAsync(user, password)` → password **PBKDF2 ile hash**lenip `AspNetUsers.PasswordHash`'e yazılır. Ardından `AddToRoleAsync` (rol) + `signInManager.SignInAsync` (cookie üret).
- **Login:** `signInManager.PasswordSignInAsync(...)` → hash karşılaştırması + cookie üretimi. Başarılıysa `HttpContext.User` sonraki isteklerde cookie'den hidrate edilir.
- **Authorization:** Controller'lara `[Authorize(Roles="...")]` filter'ı koydum. Yetkisiz istek → `ConfigureApplicationCookie`'deki `LoginPath`/`AccessDeniedPath`'e redirect (challenge/forbid).
- **Güvenlik eklentilerim:**
  - **Rate limiting:** IP başına başarısız deneme sayacı (static `Dictionary`) → brute-force throttle. *(Sınırı biliyorum: in-memory + tek instance; dağıtık ortamda `IDistributedCache`/Redis gerekir.)*
  - **Cross-gate reddi:** personel müşteri login'ini kullanamaz (role check).
  - **`AktifMi` flag:** soft-disable; pasif kullanıcı login olamaz.
- **Validation:** `RegisterViewModel`'de `[Required]`, `[EmailAddress]`, `[RegularExpression]` (güçlü şifre), `[Compare]`. Model binding sonrası `ModelState.IsValid` ile kontrol; client-side tarafı jQuery Unobtrusive Validation ile aynı attribute'lardan üretiliyor.

### 🎓 Hocaya neyi anlatmalıyım
1. **"Şifreyi ben hash'lemiyorum; Identity PBKDF2 + salt ile yapıyor"** — düz metin yok, rainbow-table'a dayanıklı.
2. **AuthN vs AuthZ farkını net ayırıyorum:** "Cookie ile kimlik taşınıyor (authentication scheme = Identity.Application), `[Authorize(Roles)]` ise authorization filter."
3. **CSRF'i nasıl önlediğim:** "Tüm POST'larda antiforgery token; Razor FormTagHelper otomatik üretiyor, `[ValidateAntiForgeryToken]` doğruluyor."
4. **İki katmanlı şifre politikası:** "Identity global kural 6 karakter; kayıt formunda ViewModel ile daha sıkı (8 + karmaşıklık) — defense in depth."
5. **Tasarım kararı:** "İki giriş kapısını ayrı scheme yerine role-based ayrımla yaptım; daha basit ve yeterli."

---

# 📙 BÖLÜM 3 — Admin Paneli (MVC + EF Core CRUD)

### 📖 ASP.NET Sözlüğü
| Terim | ASP.NET'te ne demek |
|---|---|
| **Model Binding** | Form/route/query verisini action parametrelerine/ViewModel'e otomatik map etme. |
| **ModelState** | Binding + validation sonucu; `IsValid` ile kontrol edilir. |
| **IActionResult** | Action'ın dönüş tipi soyutlaması: `View()`, `RedirectToAction()`, `NotFound()`, … |
| **PRG (Post-Redirect-Get)** | POST sonrası redirect → çift submit'i önler. |
| **TempData** | Redirect'ler arası tek-seferlik veri (cookie/session backed). PRG mesajları için. |
| **ViewBag / ViewData** | Controller→View arası tipsiz veri taşıma. |
| **EF Core Change Tracker** | DbContext'in entity'lerin durumunu (Added/Modified/Deleted) izlemesi. |
| **`SaveChangesAsync()`** | Tracked değişiklikleri tek transaction'da SQL'e çevirip uygulama. |
| **IQueryable / LINQ-to-Entities** | LINQ ifadeleri SQL'e **çevrilir** (deferred execution; `ToListAsync` ile tetiklenir). |

### 🔧 Mekanik
- **CRUD'lar** `AdminController`'da; `[Authorize(Roles="Admin")]` ile korunur. DI ile `UserManager`, `RoleManager`, `ApplicationDbContext` enjekte.
- **PRG pattern:** Tüm POST action'lar `RedirectToAction`'a dönüyor, sonuç mesajını `TempData["Success/Error"]` ile taşıyorum → kullanıcı refresh'te formu tekrar göndermiyor.
- **EF Core CRUD örnekleri:**
  - Read + filtre: `_userManager.Users.Where(u => u.Email.Contains(search))` → IQueryable, SQL'e çevrilir.
  - Update: entity'yi çek → property'leri değiştir → `SaveChangesAsync()` (change tracker UPDATE üretir; tüm kolonu değil değişeni).
  - Delete guard: `DeleteUser`'da önce `_context.Tickets.Any(t => t.MusteriId == id ...)` ile referans kontrolü → FK ihlalini exception'a bırakmadan, kullanıcıya anlamlı mesajla engelliyorum.
- **Validation döngüsü:** `if (!ModelState.IsValid) return View(model)` → hatalı formu, hata mesajlarıyla geri render.

### 🎓 Hocaya neyi anlatmalıyım
1. **EF Core'un çalışma modeli:** "LINQ sorgularım IQueryable; `ToListAsync` çağrılana kadar SQL'e gitmiyor (deferred execution). N+1'den kaçınmak için `Include` kullandım."
2. **PRG pattern'i bilinçli uyguladım:** "POST→Redirect→GET; TempData ile tek seferlik mesaj. Çift submit ve refresh sorununu çözüyor."
3. **Veri bütünlüğü:** "Silmeden önce referans kontrolü yapıyorum; FK Restrict zaten DB seviyesinde koruyor ama exception yerine kullanıcı dostu mesaj veriyorum."
4. **Change tracker:** "Update'te entity'yi çekip değiştiriyorum; EF sadece değişen kolonlar için UPDATE üretiyor."
5. **Mass assignment'a dikkat:** "Entity'yi değil ViewModel'i bind ediyorum — over-posting/over-binding riskini azaltıyor."

---

# 📕 BÖLÜM 4 — Feedback Modülü (Anonim erişim + Model Binding)

### 📖 ASP.NET Sözlüğü
| Terim | ASP.NET'te ne demek |
|---|---|
| **Anonymous access** | `[Authorize]` yok → herkes (giriş yapmadan) erişir. Varsayılan davranış. |
| **`[AllowAnonymous]`** | Korumalı bir alanda istisna açmak için (burada gerek yok, controller zaten korumasız). |
| **DbSet<T>** | DbContext'te bir entity setini (tablo) temsil eden `IQueryable` kaynağı. |
| **Nullable reference (`?`)** | Opsiyonel alan; binding'de boş gelebilir. |

### 🔧 Mekanik
- `FeedbackController` üzerinde `[Authorize]` **yok** → anonim erişim. Form `FeedbackViewModel`'e bind edilir, `ModelState` ile valide edilir, `_context.Feedbacks.Add(...) + SaveChangesAsync()`.
- `Feedback` entity'si **bağımsız** (FK'sız) — sadece `DbSet<Feedback>` olarak DbContext'e eklendi.
- Admin tarafında okundu/sil işlemleri yine PRG + TempData.

### 🎓 Hocaya neyi anlatmalıyım
- **"Authorization opt-out değil opt-in'i bilinçli yönettim":** "Identity korumayı action/controller bazında `[Authorize]` ile ekliyorum; Feedback'i kasıtlı anonim bıraktım."
- **"Entity ilişkisiz olabilir":** "Her tablo ilişki gerektirmez; Feedback bağımsız bir aggregate."

---

# 📒 BÖLÜM 5 — Güvenlik, Hata Yönetimi & Deployment

### 📖 ASP.NET Sözlüğü
| Terim | ASP.NET'te ne demek |
|---|---|
| **Exception Handling Middleware** | `UseExceptionHandler("/Home/Error")` — prod'da hataları yakalayıp özel sayfaya yönlendirir. |
| **Developer Exception Page** | Dev ortamında detaylı stack trace gösteren middleware. |
| **Environment (`IWebHostEnvironment`)** | `Development`/`Production` ayrımı; `ASPNETCORE_ENVIRONMENT` ile gelir. |
| **HSTS / HTTPS Redirection** | Transport güvenliği middleware'leri. |
| **Tag Helper** | Razor'da `asp-action`, `asp-for` gibi sunucu taraflı HTML üreten yardımcılar (role-based nav burada). |
| **CI/CD (GitHub Actions)** | Push tetikli build → publish → deploy pipeline'ı (`.github/workflows`). |
| **Azure App Service** | PaaS hosting; `D:\home` kalıcı, `wwwroot` deploy'da değişir. |

### 🔧 Mekanik
- **Ortam bazlı davranış:** `if (!app.Environment.IsDevelopment())` → prod'da `UseExceptionHandler` + `UseHsts`; dev'de developer exception page.
- **Role-based UI:** `_Layout.cshtml`'de `User.IsInRole("Admin")` ile menü koşullu render — yetkisiz linkleri hiç basmıyorum (defense in depth; sunucu tarafı `[Authorize]` zaten asıl koruma).
- **CI/CD:** `main`'e push → GitHub Actions `dotnet build` + `dotnet publish` → `azure/webapps-deploy` ile App Service'e zip deploy.
- **Çözdüğüm gerçek sorun (anlatması değerli):** SQLite dosyası `wwwroot` altında oluşuyordu; her deploy `wwwroot`'u değiştirdiği için **veri uçuyordu**. Çözüm: `WEBSITE_SITE_NAME` environment variable'ı ile Azure'ı algılayıp connection string'i kalıcı `D:\home\data`'ya yönlendirdim. Lokal etkilenmiyor (env yok).

### 🎓 Hocaya neyi anlatmalıyım
1. **Ortam ayrımı:** "Dev'de developer exception page, prod'da generic error page — bilgi sızıntısını önlüyor."
2. **CI/CD anlıyorum:** "Push'ta otomatik build+deploy; manuel adım yok."
3. **Gerçek bir prod sorununu teşhis edip çözdüm:** SQLite + ephemeral wwwroot → persistent storage'a taşıma. **Bu, framework + deployment bilgisini birlikte gösterdiği için en güçlü konuşma noktan.**
4. **SQLite'ın sınırını biliyorum:** "Tek dosya/tek instance; yatay ölçek için Azure SQL'e EF provider değişimiyle geçilir (`UseSqlServer` + connection string)."

---

# 📓 BÖLÜM 6 — Entegrasyon Testleri (WebApplicationFactory)

### 📖 ASP.NET Sözlüğü
| Terim | ASP.NET'te ne demek |
|---|---|
| **TestServer** | Uygulamayı **gerçek soket açmadan**, bellek-içi host eden test sunucusu. |
| **WebApplicationFactory<TEntryPoint>** | `Program`'ı kullanarak TestServer + gerçek DI grafiğini ayağa kaldıran fixture. |
| **Integration Test** | Birden çok katmanı (middleware+MVC+EF) gerçek HTTP ile birlikte test etme. |
| **Test Isolation** | Her fixture'a izole DB; testler birbirini etkilemez. |
| **`[Fact]` / `[Theory]`** | xUnit test attribute'ları; `[Theory]` parametrik. |
| **`public partial class Program`** | Top-level Program'ı test projesine erişilebilir kılmak için. |

### 🔧 Mekanik
- `CustomWebApplicationFactory : WebApplicationFactory<Program>` → `ConfigureServices`'te uygulamanın `DbContextOptions` kaydını **kaldırıp** test-özel benzersiz SQLite dosyasıyla yeniden kaydediyorum (gerçek DB'ye dokunmaz). `Program.cs`'in migration+seed'i test DB'sinde de çalışıyor → seed hesaplarıyla auth testi yapabiliyorum.
- **Antiforgery dansı:** Test HttpClient'ı çerezleri taşıyor; önce GET ile token'ı parse edip POST'a ekliyorum (gerçek tarayıcı davranışı).
- **Kapsam:** korumalı route'lar login'e redirect mi, register→login→Profile akışı, ticket create/close, admin delete-guard — düzelttiğimiz buglar için **regresyon** testleri dahil. 24/24 yeşil.

### 🎓 Hocaya neyi anlatmalıyım
1. **"Unit değil integration test yazdım"** — neden: "MVC+EF+middleware'i gerçek istek akışıyla doğruluyor; mock yerine TestServer."
2. **DI'ı testte override edebiliyorum:** "Factory'de DbContext kaydını söküp test DB'siyle değiştirdim — production kodu değişmeden."
3. **Test izolasyonu:** "Her fixture izole SQLite; paralel çalışmada kilitlenmeyi önlemek için DB ayrımı + seri çalıştırma."
4. **Regresyon güvencesi:** "Düzelttiğim buglar (eksik Profile view, delete-guard) için test var; tekrar gelirse pipeline kırmızı olur."

---

## 🎯 Hocaya genel mesajın (kapanış)
> "Projede ASP.NET Core'un sunduğu yapıyı bilinçli kullandım: DI ile gevşek bağlılık, middleware pipeline ile cross-cutting concerns (auth, error), Identity ile güvenli üyelik, EF Core ile code-first veri katmanı, Razor + Tag Helper ile server-side rendering. Ayrıca CI/CD ile Azure'a deploy ettim ve entegrasyon testleriyle doğruladım. Mimari trade-off'ların (servis katmanı yokluğu, SQLite sınırları) farkındayım."

## 🔑 Test Hesapları
- **Admin:** admin@helpdesk.com / Admin123! → `/staff/login`
- **Support:** support@helpdesk.com / Support123! → `/staff/login`
- **Müşteri:** `/Account/Register`
