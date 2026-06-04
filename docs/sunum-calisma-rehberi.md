# 🎓 HelpDesk — Sunum Çalışma Rehberi (Benim Bölümlerim)

> Bu doküman, projede **benim (Mert) geliştirdiğim** kısımları anlamam ve sunabilmem için hazırlandı.
> Her bölüm: **📖 Sözlük → 🔍 Anlatım → 🎤 Sunum cümlesi → ❓ Sorular** sırasıyla ilerler.

**Benim bölümlerim:** Proje İskeleti · Kimlik Doğrulama (Auth) · Admin Paneli · Feedback Sistemi · Güvenlik & Deployment · Otomatik Testler

---

# 🌐 KATMAN 0 — Hiç Bilmeyene: Bir Web Uygulaması Nasıl Çalışır?

> Aşağıyı bir https://github.com/os-hardening-ai/ai-powered-os-hardening-deploy.git
https://github.com/os-hardening-ai/ai-powered-os-hardening.git
https://github.com/os-hardening-ai/ai-powered-os-hardening-frontend.git
https://github.com/os-hardening-ai/ai-powered-os-hardening-monitoring.git
önce bizim tarafta her şeyi merge et. merge edilen branchleri sil.
şimdi bu 4 repodakii çalışmaları kendi docs dizinlerinde açıkla. adım adım. önce hangi başlıklar ve dosuyalar olmalı çıkart. Sonra her konu yani dosya başlığını denetle, var olanları incele, var olanlardaki hataları düzelt.
Eksik ve hatalı olanları topladıktan sonra olmayanları oluştur ve proje durumunu orada dookümante et. 
daha sonra bunları bitirme tez raprounda kullanacağız.
daha sonra sana bitirme projesi şablonu attım.
daha önce yaptığımız ara rapor sunumlarını 4 dosya olarak ay bazında gönderdim.
Hocalarla ilk anlaştığımız öneri formumuzu ilettim. Bunlara uygun olarak bana bitirme tez raporu için planlama hazırla.kez okursan, dokümanın geri kalanındaki her şey yerine oturur. Hiç kod bilmene gerek yok.

## Restoran benzetmesi (en kolay yol)
Bir web uygulamasını **restoran** gibi düşün:

| Restoranda | Web uygulamasında | Açıklama |
|---|---|---|
| Müşteri (sen) | **Tarayıcı / İstemci** | Chrome, telefon… senin baktığın yer |
| Sipariş vermek | **İstek (Request)** | "Bana şu sayfayı getir" |
| Garson + mutfak | **Sunucu (Server) / Backend** | Siparişi alıp hazırlayan arka taraf |
| Kiler / depo | **Veritabanı (Database)** | Tüm bilgilerin saklandığı yer |
| Gelen tabak | **Cevap (Response)** | Sana dönen hazır sayfa (HTML) |
| Tabağın sunumu | **Frontend** | Senin gözünle gördüğün ekran/tasarım |

## Adım adım: bir sayfaya girince ne oluyor?
```
1. Adres çubuğuna URL yazarsın        → "sipariş veriyorsun" (İSTEK)
2. İstek internetten SUNUCUYA gider   → "mutfağa iletiliyor"
3. Sunucu (Backend) gerekirse          → "depodan malzeme alıyor"
   VERİTABANINDAN veri çeker
4. Sunucu bir HTML sayfası hazırlar    → "yemek pişiyor"
5. Sayfa tarayıcına geri döner         → "tabak masaya geliyor" (CEVAP)
6. Ekranda görürsün                    → "yemeği yiyorsun"
```

Bu projede **mutfağı (backend)** ve **deponun düzenini (veritabanı)** ben kurdum.
Bizim mutfağımızın çalışma şekli **MVC**: gelen siparişi **Controller** alır, malzemeyi **Model** temsil eder, tabağı **View** süsler.

---

# 🔤 KATMAN 1 — En Temel 10 Terim (günlük dille)

> Bunlar dokümanın HER yerinde geçen "alfabe". Önce bunları öğren, sonra teknik sözlükler kolay gelir.

| # | Terim | Günlük dille ne demek? |
|---|---|---|
| 1 | **Sunucu (Server)** | 7/24 açık, istekleri karşılayıp cevap veren bilgisayar. (Restoranın mutfağı.) |
| 2 | **İstemci / Tarayıcı (Client)** | Senin kullandığın Chrome, Edge, telefon. (Müşteri.) |
| 3 | **İstek / Cevap (Request / Response)** | İstek = "şunu ver" sorusu; Cevap = sunucunun döndürdüğü sayfa. |
| 4 | **HTTP** | Tarayıcı ile sunucunun **konuştuğu dil/kurallar**. (Sipariş protokolü.) |
| 5 | **URL** | İnternet **adresi** (örn. `site.com/Account/Login`). |
| 6 | **Veritabanı (Database)** | Bilgilerin saklandığı **dev, düzenli Excel** gibi düşün: her tablo bir sayfa, her satır bir kayıt. |
| 7 | **SQL** | Veritabanına "**şunu getir / ekle / sil**" demenin dili. (Biz EF Core ile bunu otomatik ürettiriyoruz, elle yazmıyoruz.) |
| 8 | **Sınıf (Class) ve Nesne (Object)** | Sınıf = **kurabiye kalıbı** (şablon). Nesne = o kalıptan çıkan **tek bir kurabiye**. Örn. `ApplicationUser` kalıp, "ahmet@..." bir nesne. |
| 9 | **Attribute (etiket)** | Bir alanın üstüne yapıştırılan **kural etiketi**. Örn. `[Required]` = "bu boş olamaz". |
| 10 | **IP adresi** | İnternetteki her cihazın **telefon numarası** gibi kimliği. (Giriş limitinde "aynı numaradan çok deneme" derken bunu kastediyoruz.) |

> Mini not: **Frontend** = kullanıcının gördüğü yüz (ekran). **Backend** = arkada çalışan mantık (sunucu kodu). **Framework** = hazır iskelet/araç seti (ASP.NET Core bir framework'tür — sıfırdan her şeyi yazmazsın).

---

## 🧱 Genel Terimler (her bölümde geçer — bir kez öğren)

| Terim | Ne demek / ne işe yarar |
|---|---|
| **ASP.NET Core MVC** | Microsoft'un web framework'ü. MVC = Model-View-Controller mimarisi. |
| **MVC** | Kodu 3 parçaya ayırma yöntemi: **Model** (veri), **View** (ekran), **Controller** (mantık). |
| **Controller** | Gelen istekleri karşılayan sınıf. İçindeki her metoda **Action** denir. |
| **Action** | Controller içindeki bir metot; bir URL'e karşılık gelir (örn. `Login()` → `/Account/Login`). |
| **View (.cshtml)** | Kullanıcının gördüğü HTML sayfası. **Razor** motoruyla C# + HTML karışık yazılır. |
| **Razor** | View içinde `@` ile C# kodu çalıştırmayı sağlayan şablon motoru. |
| **Model** | Veriyi temsil eden C# sınıfı (örn. `Ticket`, `Category`). |
| **ViewModel** | Sadece bir ekran/form için hazırlanmış, sadeleştirilmiş veri taşıyıcı sınıf. |
| **Endpoint / Route** | Bir URL adresi ve onu işleyen action (örn. `/Admin/CreateUser`). |
| **Request / Response** | İstek (tarayıcıdan gelen) / Cevap (sunucunun döndürdüğü HTML). |
| **async / await** | İşlemi beklerken (örn. veritabanı) uygulamayı kilitlemeden çalıştırma yöntemi. |
| **Dependency Injection (DI)** | Servisleri (veritabanı, UserManager…) elle `new`'lemeden, hazır olarak sınıfa "enjekte" etme. |

---

# 📘 BÖLÜM 1 — Proje İskeleti & Teknoloji Kurulumu

### 📖 Bu bölümün terimleri
| Terim | Ne demek / ne işe yarar |
|---|---|
| **Program.cs** | Uygulamanın **başlangıç noktası**; ilk çalışan dosya. Her şeyi burada kuruyoruz. |
| **builder.Services** | Uygulamaya servis (araç) **kaydettiğimiz** yer. ("Şu araçları kullanacağım.") |
| **Middleware** | Gelen isteğin **tek tek geçtiği kontrol noktaları** (https, routing, auth…). |
| **Pipeline** | Middleware'lerin **sıralı dizisi**. İstek baştan sona bu hattan geçer. |
| **Entity Framework Core (EF Core)** | C# nesnelerini veritabanı tablolarına çeviren **ORM** aracı. |
| **ORM** | Object-Relational Mapping: SQL yazmadan, C# nesneleriyle veritabanı işlemi yapma. |
| **DbContext** | Kod ile veritabanı arasındaki **çevirmen** sınıfı (`ApplicationDbContext`). |
| **SQLite** | Tek dosyalık (`helpdesk.db`), sunucu gerektirmeyen küçük veritabanı. |
| **Connection String** | Veritabanına nasıl bağlanılacağını söyleyen metin (`Data Source=helpdesk.db`). |
| **Migration** | Model değişikliğini veritabanı tablosuna yansıtan **sürümlü adım**. |
| **Seed (tohum veri)** | Veritabanı ilk kurulduğunda otomatik eklenen **başlangıç verileri** (admin, roller…). |
| **appsettings.json** | Uygulama **ayar dosyası** (bağlantı dizesi vb. burada). |
| **Code-First** | Önce C# sınıflarını yaz, EF Core veritabanını ona göre oluştursun yaklaşımı. |

### 🔍 Anlatım
**Program.cs** uygulamanın ilk çalışan dosyasıdır ve iki ana parçadan oluşur:
1. **Servis kaydı** (`builder.Services...`) → "Hangi araçlar olacak?" (veritabanı, Identity, MVC)
2. **Pipeline kurulumu** (`app.Use...`) → "İstek hangi adımlardan geçecek?"

Arada `var app = builder.Build();` ile ayrılır. Üstü kurulum, altı çalışma.

**Yaptıklarım sırayla:**
- **Veritabanı:** `AddDbContext` + `UseSqlite` ile SQLite bağlantısını kurdum. `ApplicationDbContext` kod ↔ veritabanı çevirmenidir.
- **Identity:** `AddIdentity` ile üyelik sistemini açtım (şifre kuralları burada).
- **Cookie:** Giriş yapmamış kullanıcı korumalı sayfaya girince `/Account/Login`'e yönlendirilir.
- **Pipeline:** `UseAuthentication` (kimsin?) → `UseAuthorization` (yetkin var mı?) sırası **önemli**.
- **Otomatik kurulum:** `db.Database.Migrate()` veritabanını otomatik kurar, `SeedData` ilk verileri ekler.

> **Code-First mantığı:** Önce C# sınıflarını (Model) yazdım, EF Core bunları tablolara çevirdi (`Migrations/` klasörü).

### 🎤 Sunum cümlesi
> "Program.cs'te üç şeyi kurdum: SQLite veritabanını EF Core ile, ASP.NET Core Identity ile üyelik/rolleri, ve isteklerin geçtiği güvenlik akışını. Uygulama her açıldığında veritabanı otomatik kuruluyor ve admin/roller/kategoriler otomatik ekleniyor — manuel SQL yok."

### ❓ Sorular
- **"Startup.cs nerede?"** → ".NET 6+ ile minimal hosting geldi, Program.cs ile birleşti."
- **"Migration'ı kim çalıştırıyor?"** → "`db.Database.Migrate()` açılışta otomatik."
- **"Neden SQLite?"** → "Kurulumsuz, tek dosya; EF Core ile tek satırda Azure SQL'e geçilebilir."

---

# 📗 BÖLÜM 2 — Kimlik Doğrulama (Authentication)

### 📖 Bu bölümün terimleri
| Terim | Ne demek / ne işe yarar |
|---|---|
| **Authentication (Kimlik Doğrulama)** | "Sen kimsin?" — giriş kontrolü. |
| **Authorization (Yetkilendirme)** | "Bunu yapmaya hakkın var mı?" — rol/yetki kontrolü. |
| **ASP.NET Core Identity** | Üyelik, giriş, şifre, rol işlerini yapan hazır sistem. |
| **ApplicationUser** | Identity'nin standart kullanıcısına bizim alanlarımızı (AdSoyad, Departman) eklediğimiz sınıf. |
| **UserManager** | Kullanıcı işlemleri: oluştur, bul, rol ata, sil, güncelle. |
| **SignInManager** | Oturum işlemleri: şifre doğrula, giriş yap, çıkış yap. |
| **Role (Rol)** | Kullanıcı tipi: Admin, SupportAgent, Customer. |
| **Cookie (Çerez)** | Giriş bilgisini tutan şifreli küçük dosya (tarayıcıda). |
| **Hash / Hashing** | Şifreyi geri döndürülemez şekilde şifreleyip saklama (düz metin tutulmaz). |
| **DataAnnotations** | Model alanlarına kural koyan attribute'lar (`[Required]`, `[EmailAddress]`…). |
| **ModelState** | Formun doğrulama sonucunu tutan nesne. `IsValid` true ise kurallar geçti demektir. |
| **[ValidateAntiForgeryToken]** | Formun sahte/dışarıdan gönderilmediğini doğrular (**CSRF** koruması). |
| **CSRF** | Cross-Site Request Forgery: kullanıcı adına sahte istek gönderme saldırısı. |
| **[Authorize]** | Bir sayfaya sadece giriş yapanların / belirli rollerin girmesini sağlar. |
| **Rate Limiting** | Belirli sürede çok fazla deneme yapanı engelleme (brute-force koruması). |
| **Self-registration** | Kullanıcının kendi kendine kayıt olması (admin onayı gerekmeden). |
| **[Compare]** | İki alanın eşit olmasını kontrol eden kural (şifre = şifre tekrarı). |

### 🔍 Anlatım
**İki kapı tasarımı:** Müşteriler `/Account/Login`, personel `/staff/login`. Bir personel müşteri kapısından girmeye çalışırsa engelleyip doğru kapıya yönlendiriyorum.

**Kayıt (Register):** Form gelir → `ModelState.IsValid` ile doğrulanır → `ApplicationUser` oluşur → `UserManager.CreateAsync` ile DB'ye yazılır (**şifre hash'lenir**) → otomatik **Customer** rolü atanır → otomatik giriş yapılır.

**Giriş (Login) — 3 güvenlik katmanı:**
1. **Rate limiting:** Aynı IP'den 15 dk içinde 5 başarısız giriş → kilit.
2. **Personel ayrımı:** Admin/SupportAgent müşteri girişini kullanamaz.
3. **Pasif hesap kontrolü:** `AktifMi == false` ise giriş engellenir.

**Personel girişi (StaffController):** Aynı mantığın tersi — sadece Admin/SupportAgent kabul edilir, başarılı girişte `/Support/Dashboard`'a gider.

**Form doğrulama:** Kayıt formunda güçlü şifre zorunlu (`RegularExpression` ile en az 8 karakter, büyük+küçük harf, rakam, özel karakter) ve `[Compare]` ile şifre tekrarı kontrolü.

### 🎤 Sunum cümlesi
> "Kimlik doğrulamayı ASP.NET Core Identity ile yaptım. Müşteriler kendi kayıt oluyor ve otomatik Customer rolü alıyor; personel ayrı kapıdan giriyor. Şifreler hash'leniyor, güçlü şifre zorunlu, ve brute-force'a karşı giriş deneme limiti koydum. Yetkilendirmeyi `[Authorize(Roles=...)]` ile sağladım."

### ❓ Sorular
- **"Şifreler nasıl saklanıyor?"** → "Identity hash'liyor; ben sadece `CreateAsync`'e veriyorum."
- **"UserManager / SignInManager farkı?"** → "Biri kullanıcıyı yönetir, diğeri oturumu."
- **"Rate limiting nasıl?"** → "IP başına deneme sayısını tutuyorum, 5'ten sonra 15 dk kilit."
- **"Giriş bilgisi nerede?"** → "Şifreli çerezde (cookie)."

---

# 📙 BÖLÜM 3 — Admin Paneli

### 📖 Bu bölümün terimleri
| Terim | Ne demek / ne işe yarar |
|---|---|
| **CRUD** | Create-Read-Update-Delete: oluştur-oku-güncelle-sil işlemleri. |
| **[Authorize(Roles="Admin")]** | Bu controller'a sadece Admin rolü girebilir. |
| **RoleManager** | Rolleri yöneten Identity servisi (rol oluştur/kontrol). |
| **TempData** | Bir sonraki sayfaya tek seferlik mesaj taşıma (örn. "Kullanıcı silindi"). |
| **ViewBag** | Controller'dan View'a hızlıca veri taşıma (dinamik). |
| **LINQ** | C# içinde veri sorgulama dili (`Where`, `Count`, `OrderBy`, `GroupBy`). |
| **Foreign Key (FK) / Restrict** | Tablolar arası bağ; **Restrict** = bağlı kayıt varsa silmeyi engeller. |
| **Partial View** | Tekrar kullanılan view parçası (örn. `_AdminSidebar`). |
| **Dashboard** | İstatistik/özet ekranı (kart ve sayılar). |
| **IsValid / Validation** | Gelen verinin kurallara uyup uymadığının kontrolü. |

### 🔍 Anlatım
Admin paneliyle sistem yöneticisi her şeyi yönetir. `[Authorize(Roles="Admin")]` ile **sadece admin** erişir.

**Kullanıcı yönetimi (CRUD):**
- **Dashboard / Index:** İstatistik kartları (toplam kullanıcı, admin, agent, müşteri) + kullanıcı listesi + **arama** (LINQ `Where` ile isim/email).
- **CreateUser:** Yeni personel oluştur (rol + departman seçerek).
- **EditRole:** Rol değiştir. **ToggleActive:** Aktif/pasif yap.
- **DeleteUser:** Sil — ama **talebi olan kullanıcı silinemiyor** (FK Restrict + uygulama kontrolü). Ayrıca admin kendini silemez.

**Kategori yönetimi (CRUD):** Kategori ekle / aktif-pasif / sil. Talebi olan kategori silinemiyor.

**Feedback yönetimi:** Gelen geri bildirimleri listele, "okundu" işaretle, sil.

**Tüm Talepler:** Sistemdeki bütün talepleri durum filtresiyle görüntüle.

> Her POST işleminde `[ValidateAntiForgeryToken]` (CSRF koruması) ve işlem sonrası `TempData` ile kullanıcıya mesaj.

### 🎤 Sunum cümlesi
> "Admin paneliyle yönetici kullanıcıları, rolleri, kategorileri ve geri bildirimleri yönetiyor. Tüm işlemler CRUD mantığında; veri bütünlüğü için talebi olan kullanıcı veya kategori silinemiyor, admin kendini silemiyor."

### ❓ Sorular
- **"Arama nasıl çalışıyor?"** → "LINQ `Where` ile isim/email içinde filtreliyorum."
- **"Talebi olan kullanıcıyı silersen?"** → "Engelliyorum; önce talebi var mı kontrol ediyorum, varsa hata mesajı."
- **"TempData ne?"** → "Yönlendirme sonrası tek seferlik bilgi mesajı taşıyor."

---

# 📕 BÖLÜM 4 — Feedback (Geri Bildirim) Sistemi

### 📖 Bu bölümün terimleri
| Terim | Ne demek / ne işe yarar |
|---|---|
| **Public / Anonim erişim** | Giriş yapmadan, herkesin erişebildiği sayfa. |
| **Model Binding** | Formdaki alanları otomatik olarak C# nesnesine (ViewModel) doldurma. |
| **nullable (`?`)** | Boş olabilen alan (örn. `string? Email` → email girilmesi şart değil). |
| **DbSet** | DbContext içinde bir tabloyu temsil eden koleksiyon (`Feedbacks`). |
| **Bağımsız tablo** | Başka tabloya yabancı anahtarı (FK) olmayan tablo. |

### 🔍 Anlatım
Üyelik gerektirmeyen bir geri bildirim modülü ekledim:
- **`/Feedback/Create`:** Herkes (giriş yapmadan) form doldurabilir — kategori (Geri Bildirim / Öneri / Şikayet), mesaj, opsiyonel email.
- Gönderilen veri `Feedback` tablosuna kaydedilir. Bu tablo **bağımsızdır** (hiçbir tabloya bağlı değil).
- Admin, `/Admin/Feedbacks` sayfasından bunları görür, **"okundu"** işaretler veya siler.

> `Model Binding` sayesinde form alanları otomatik olarak `FeedbackViewModel`'e dolar; `ModelState.IsValid` ile doğrulanır.

### 🎤 Sunum cümlesi
> "Kullanıcıların görüş bildirebilmesi için üyelik gerektirmeyen bir geri bildirim modülü yaptım. Gelen geri bildirimleri admin panelinden yönetiyorum: okundu işaretleme ve silme."

### ❓ Sorular
- **"Neden üyelik gerekmiyor?"** → "Geri bildirim genel bir özellik; herkes ulaşabilsin istedim."
- **"Email opsiyonel mi?"** → "Evet, `nullable`; kullanıcı isterse dönüş için bırakır."

---

# 📒 BÖLÜM 5 — Güvenlik & UX + Deployment

### 📖 Bu bölümün terimleri
| Terim | Ne demek / ne işe yarar |
|---|---|
| **Role-based UI** | Menünün/sayfanın kullanıcının rolüne göre değişmesi. |
| **Custom Error Page** | Hata durumunda (404/500) gösterilen özel tasarlanmış sayfa. |
| **404 / 500** | 404 = sayfa bulunamadı, 500 = sunucu hatası. |
| **CI/CD** | Continuous Integration/Deployment: kodu otomatik derleyip yayınlama süreci. |
| **GitHub Actions** | GitHub'ın otomasyon aracı; push'ta otomatik build+deploy yapar. |
| **Azure App Service** | Microsoft'un web uygulaması barındırma (hosting) servisi. |
| **Deployment** | Uygulamayı canlı sunucuya yayınlama. |
| **Persistent Storage** | Kalıcı disk alanı (deploy'da silinmeyen). Azure'da `D:\home`. |
| **Environment Variable** | Ortam değişkeni; çalışma ortamına göre değişen ayar (örn. `WEBSITE_SITE_NAME`). |
| **HSTS / HTTPS Redirect** | Bağlantıyı güvenli (https) hale getiren ayarlar. |

### 🔍 Anlatım
**Güvenlik & UX:**
- **Role-based navbar:** Menü role göre değişir (müşteriye "Taleplerim", admine "Admin paneli").
- **Özel hata sayfaları:** 404 ve 500 için tasarlanmış sayfalar.
- **Giriş deneme limiti** (Bölüm 2'deki rate limiting).

**Deployment (CI/CD):**
- **GitHub Actions** ile `main`'e her push'ta uygulama otomatik **build → publish → Azure App Service'e deploy** ediliyor (`.github/workflows/main_ticket-sistemi.yml`).
- **Azure DB sorunu ve çözümü (artı puan):** SQLite dosyası varsayılan olarak `wwwroot` içinde oluşuyordu ve her deploy'da silindiği için veri kayboluyordu. Çözüm: Azure'da veritabanını **kalıcı klasöre** (`D:\home\data`) taşıdım. Bunu `WEBSITE_SITE_NAME` ortam değişkenini kontrol ederek sadece Azure'da devreye giren bir kod ile yaptım (lokal etkilenmiyor).

### 🎤 Sunum cümlesi
> "Güvenlik tarafında rol bazlı menü, özel hata sayfaları ve giriş limiti var. Projeyi GitHub Actions ile Azure'a otomatik deploy ettim — main'e her push'ta yayınlanıyor. Deploy sürecinde SQLite'ın silinme sorununu, veritabanını Azure'ın kalıcı diskine taşıyarak çözdüm."

### ❓ Sorular
- **"Deploy nasıl çalışıyor?"** → "GitHub Actions push'ta build alıp Azure App Service'e gönderiyor."
- **"Her push'ta veri siliniyor muydu?"** → "Evet, SQLite wwwroot'taydı; kalıcı klasöre taşıyarak çözdüm."
- **"CI/CD ne?"** → "Kodun otomatik derlenip yayınlanması; manuel deploy yok."

---

# 📓 BÖLÜM 6 — Otomatik Testler

### 📖 Bu bölümün terimleri
| Terim | Ne demek / ne işe yarar |
|---|---|
| **Unit Test** | Tek bir parçayı (fonksiyon) izole test etme. |
| **Integration Test** | Birden çok parçayı birlikte, gerçek akışla test etme (bizim yaptığımız). |
| **xUnit** | .NET'in popüler test framework'ü. |
| **WebApplicationFactory** | Uygulamayı bellek-içi başlatıp gerçek HTTP istekleriyle test etmeyi sağlar. |
| **Test Isolation** | Her testin kendi izole verisiyle çalışması (birbirini etkilememesi). |
| **Assertion (`Assert`)** | "Sonuç şu olmalı" kontrolü; tutmazsa test başarısız. |
| **Regression Test** | Düzeltilen bir hatanın **tekrar oluşmadığını** garanti eden test. |
| **[Fact] / [Theory]** | Test metodu işaretleri. `[Theory]` farklı verilerle aynı testi çalıştırır. |

### 🔍 Anlatım
Kodun güvenilir olması için **24 otomatik entegrasyon testi** yazdım (`HelpDesk.Tests` projesi):
- **WebApplicationFactory** ile uygulamayı bellek-içi başlatıp **gerçek HTTP istekleri** atıyorum.
- Her test **izole geçici SQLite** kullanıyor (gerçek veriye dokunmuyor).
- **FE testleri:** sayfalar doğru HTML/form/navbar ile render oluyor mu?
- **BE testleri:** yetkisiz erişim login'e yönleniyor mu, talep oluşturma/kapatma çalışıyor mu, yetki kuralları doğru mu?
- Düzelttiğimiz hatalar için **regresyon testleri** (örn. Profile sayfası, kullanıcı silme koruması).
- Çalıştırma: `dotnet test` → 24 yeşil.

### 🎤 Sunum cümlesi
> "Kodun doğruluğunu garanti etmek için 24 otomatik entegrasyon testi yazdım. Gerçek HTTP istekleriyle giriş, yetki ve talep akışlarını test ediyor; her test izole veritabanı kullanıyor. `dotnet test` ile hepsi geçiyor."

### ❓ Sorular
- **"Unit mi integration mı?"** → "Integration — gerçek istekle uçtan uca."
- **"Testler gerçek DB'yi bozar mı?"** → "Hayır, her test kendi geçici SQLite'ını kullanıyor."
- **"Regresyon testi ne?"** → "Düzelttiğim bir hatanın tekrar gelmediğini garanti eden test."

---

## ✅ Hızlı Demo Sırası (sunumda ekranda)
1. `dotnet run` → ana sayfa
2. Müşteri **kayıt ol** → otomatik giriş → talep oluştur
3. Çıkış → `/staff/login` → **admin** girişi
4. Admin panel: kullanıcı oluştur, kategori ekle, feedback'leri gör
5. `dotnet test` → 24 yeşil test

## 🔑 Test Hesapları
- **Admin:** admin@helpdesk.com / Admin123! (giriş: `/staff/login`)
- **Support:** support@helpdesk.com / Support123! (giriş: `/staff/login`)
- **Müşteri:** `/Account/Register`'dan kendin oluştur
