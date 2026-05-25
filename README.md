# HelpDesk - Teknik Destek Ticket Sistemi

## Proje Açıklaması

ASP.NET Core MVC tabanlı, 4 kişilik takım tarafından geliştirilen teknik destek ticket yönetim sistemi.

## Teknoloji Stack

- **.NET**: 10.0
- **Framework**: ASP.NET Core MVC
- **Veritabanı**: SQLite
- **ORM**: Entity Framework Core 9.0.1
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5, HTML/CSS

## NuGet Paketleri

```
Microsoft.EntityFrameworkCore.Sqlite 9.0.1
Microsoft.EntityFrameworkCore.Tools 9.0.1
Microsoft.AspNetCore.Identity.EntityFrameworkCore 9.0.1
```

## Klasör Yapısı

```
├── Controllers/          # MVC Controllers
├── Models/              # Domain Models (ApplicationUser, vb.)
├── Views/               # Razor Views
├── ViewModels/          # View Models (InputModels)
├── Data/                # DbContext, Migrations, SeedData
├── Services/            # Business Logic Services
├── wwwroot/             # Static Files (CSS, JS, Images)
├── Properties/          # Project Properties
└── appsettings.json     # Configuration
```

## Kurulum & Çalıştırma

### 1. Gerekli Tools
- .NET 10.0 SDK
- Visual Studio 2022 (veya VS Code)

### 2. Bağımlılıkları Yükle
```bash
dotnet restore
```

### 3. Veritabanı Migrations (otomatik çalışır)
```bash
dotnet ef database update
```

### 4. Projeyi Çalıştır
```bash
dotnet run
```

Site: `https://localhost:5001` veya `http://localhost:5000`

## Test Hesapları

### 👤 Müşteri Portali (`/` - Ana site)

**Kayıt Ol** (Register)
- URL: `http://localhost:5000/Account/Register` veya `http://localhost:5000/Account/Login` → "Kayıt Ol" linki
- İşlem: Form doldur → Customer rolü otomatik atanır, anında aktif olur
- Test: `testuser@example.com` / `TestPass123!`

**Giriş Yap** (Login)
- URL: `http://localhost:5000/Account/Login`
- Müşteri hesabıyla giriş, kendi ticket'larını görebilir

### 👨‍💼 Personel Girişi (`/staff/login` - Destek Ekibi)

**Giriş Yap** (Staff Login)
- URL: `http://localhost:5000/staff/login`
- Sadece Admin ve SupportAgent rolleri giriş yapabilir
- Başarılı giriş → Support Dashboard'a yönlendir

### 🔧 Admin Paneli

**Admin Hesabı** (Seed'den otomatik oluşturulur)
- **Email**: admin@helpdesk.com
- **Şifre**: Admin123!
- **Rol**: Admin
- **Giriş**: `/staff/login` adresinden
- **Erişim**: Admin Panel (`/Admin`) - Kullanıcı yönetimi

**SupportAgent (Örnek Hesap)**
- **Email**: support@helpdesk.com
- **Şifre**: Support123!
- **Rol**: SupportAgent
- **Departman**: Teknik Destek (admin tarafından atanmış)
- **Giriş**: `/staff/login` adresinden
- **Erişim**: Support Dashboard (`/Support/Dashboard`) - Ticket yönetimi

### Yeni Kullanıcı Oluşturma (Admin Panelinden)

Admin, `/Admin/CreateUser` sayfasından yeni kullanıcı oluşturabilir:
1. Ad Soyad, Email, Telefon gir
2. Rol seç: Customer, SupportAgent, Admin
3. SupportAgent/Admin seçilirse → Departman seçeneği açılır
4. Şifre belirle
5. "Oluştur" → Kullanıcı anında aktif olur, giriş yapabilir

## Roller

1. **Admin** - Sistem yöneticisi, tüm özelliklere erişim
2. **Customer** - Müşteri, kendi ticket'larını görüntüleme/oluşturma
3. **SupportAgent** - Destek temsilcisi, ticket'ları işleme

## Git Branch Stratejisi

- **main** - Production branch, stable kodlar
- **dev** - Entegrasyon branch'i
- **kisi1**, **kisi2**, **kisi3** - Feature branch'leri (her kişi kendi branch'inde çalışır)

### Branch Workflow

```bash
# Kişi 1 - feature branch'ini başlat
git checkout -b kisi1

# Değişiklikler yap, commit et
git add .
git commit -m "feat: ..."

# Push ve PR aç
git push origin kisi1
# GitHub'da PR oluştur: kisi1 -> dev
```

Faz 0 tamamlandığında:
```bash
git checkout main
git merge dev
```

## Proje Aşamaları

### Faz 0 - İskelet (Kişi 1)
- ✅ NuGet paketleri
- ✅ Klasör yapısı
- ✅ ApplicationUser modeli
- ✅ ApplicationDbContext
- ✅ Identity konfigürasyonu
- ✅ SeedData (Roller + Admin kullanıcısı)
- ✅ Migrations
- ✅ Boş Controller sınıfları
- ✅ _Layout.cshtml temel hali
- ✅ README.md

### Faz 1 - Auth + Admin Paneli (Kişi 1) ✅ TAMAMLANDI

#### Müşteri Tarafı (`/` - Public)
- ✅ `/Account/Register` - Müşteri self-registration (Customer rolü otomatik atanır)
- ✅ `/Account/Login` - Müşteri giriş
- ✅ `/Account/Logout` - Çıkış
- ✅ Navbar dinamik menü (role-based visibility)
- ✅ Home page

#### Personel Tarafı (`/staff`)
- ✅ `/staff/login` - Personel giriş (SupportAgent + Admin için)
- ✅ StaffController - Personel login işlemleri
- ✅ Personel login view'ı (müşteri login'den ayrı)

#### Admin Tarafı (`/Admin` - Sadece Admin rolüne erişim)
- ✅ `/Admin/Index` - Kullanıcı listesi (arama özelliği)
- ✅ `/Admin/CreateUser` - Yeni kullanıcı oluşturma (rol + departman seçimi)
- ✅ `/Admin/EditRole` - Kullanıcı rolü değiştirme
- ✅ `/Admin/ToggleActive` - Kullanıcı aktif/pasif yapma
- ✅ `/Admin/DeleteUser` - Kullanıcı silme
- ✅ Admin dashboard view'ları (istatistik kartları)
- ✅ `ApplicationUser.Departman` alanı (staff için opsiyonel)

#### Database & Models
- ✅ ApplicationUser (Departman alanı eklendi)
- ✅ Migration: `AddDepartmanToUser`
- ✅ Seed Data: Admin hesabı otomatik oluşturulur
- ✅ CreateUserViewModel (rol + departman seçimi için)

### Faz 2 - Model Tasarımı + Müşteri Tarafı (Kişi 2) 🔄 DEVAM EDIYOR

#### Models
- [ ] `Models/Category.cs` - Kategori modeli (Id, Ad, Aciklama)
- [ ] `Models/Ticket.cs` - Ticket modeli
  - Özellikleri: Id, Baslik, Aciklama, Status (enum), Priority (enum), CreatedAt, UpdatedAt
  - Foreign Keys: CategoryId, CustomerId, AssignedAgentId (nullable)
- [ ] `Models/TicketReply.cs` - Ticket yanıtı (Id, Mesaj, TicketId, UserId, CreatedAt)
- [ ] Enums: `TicketStatus` (Acik, Cevaplandi, Cozuldu, Kapandi), `TicketPriority` (Dusuk, Orta, Yuksek)

#### Database & Context
- [ ] `ApplicationDbContext.cs` - DbSet<Ticket>, DbSet<TicketReply>, DbSet<Category> ekle
- [ ] Fluent API konfigürasyonu (FK ilişkileri)
- [ ] Migration oluştur ve uygula
- [ ] `Data/SeedData.cs` - Örnek kategoriler seed et

#### Customer Controllers & Views
- [ ] `Controllers/TicketsController.cs`
  - `Index()` - Müşterinin kendi ticket'larını listele (filtre + arama)
  - `Create()` - Yeni ticket oluştur (kategori + açıklama + öncelik)
  - `Details(id)` - Ticket detayı + ticket yanıtları
  - `Reply(ticketId)` - Ticket'a yanıt ekle
  - `Close(ticketId)` - Ticket'ı kapat
- [ ] ViewModels: `TicketCreateViewModel`, `TicketDetailViewModel`, `TicketListItemViewModel`
- [ ] Views:
  - `Views/Tickets/Index.cshtml` - Ticket listesi
  - `Views/Tickets/Create.cshtml` - Yeni ticket formu
  - `Views/Tickets/Details.cshtml` - Ticket detayı + yanıt alanı

### Faz 3 - UI + Destek Ekibi Tarafı (Kişi 3) 🔄 PLANLAMA

#### Frontend (Kişi 2'yi beklemeden başlayabilir)
- [ ] `Views/Shared/_Layout.cshtml` - Final tasarım (kurumsal görünüm)
- [ ] `wwwroot/css/site.css` - Global stiller, responsive design
- [ ] `Views/Shared/_AlertPartial.cshtml` - Ortak alert/toast view'ı

#### Backend (Kişi 2'den sonra)
- [ ] `Controllers/SupportController.cs`
  - `Dashboard()` - Destek aşamasi istatistikleri (açık/kapalı/atanmış)
  - `Index(filters)` - Ticket listesi (durum/kategori/öncelik/arama filtresi)
  - `AssignedToMe()` - Bana atanan ticket'lar
  - `Assign(ticketId)` - Ticket atama
  - `UpdateStatus(ticketId)` - Ticket durumu güncelle
  - `Reply(ticketId)` - Ticket'a yanıt ekle

#### Frontend (Kişi 2 sonrası)
- [ ] ViewModels: `SupportDashboardViewModel`, `TicketFilterViewModel`
- [ ] Views:
  - `Views/Support/Dashboard.cshtml` - Destek panosu (istatistikler + bekleyen talepler)
  - `Views/Support/Index.cshtml` - Filtreleme UI + ticket listesi
  - `Views/Support/AssignedToMe.cshtml` - Bana atanmış ticket'lar

## Kontribütörler

- **Kişi 1**: İskelet + Auth + Admin
- **Kişi 2**: Model Tasarımı + Müşteri Tarafı
- **Kişi 3**: UI + Destek Tarafı
- **Kişi 4**: TBD

## Sorunlar & Notlar

- SQLite `helpdesk.db` dosyası proje root'unda otomatik oluşturulur
- İlk run'da veritabanı otomatik migrate edilir
- Roles ve admin kullanıcısı otomatik seed edilir

## Durum Özeti (26 Mayıs 2026)

### ✅ Tamamlanan Özellikler:

#### Faz 0 - İskelet ✅
- NuGet paketleri (EF Core 9, Identity, SQLite)
- Klasör yapısı
- ApplicationDbContext
- Identity konfigürasyonu
- Migrations altyapısı

#### Faz 1 - Authentication & Admin Paneli ✅
- **Müşteri Kayıt Sistemi**
  - `/Account/Register` - Self-registration (Customer rolü otomatik)
  - `/Account/Login` - Müşteri girişi
  - `/Account/Logout` - Çıkış
- **Personel Girişi**
  - `/staff/login` - Admin + SupportAgent için ayrı login endpoint
  - StaffController.cs - Personel giriş mantığı
- **Admin Paneli**
  - Kullanıcı listesi (arama, filtre)
  - Yeni kullanıcı oluşturma (rol + departman seçimi)
  - Rol değiştirme
  - Kullanıcı aktif/pasif yapma
  - Kullanıcı silme
  - İstatistik kartları (toplam/admin/agent/müşteri)
- **Role-Based UI**
  - Navbar dinamik (role göre menü öğeleri göster/gizle)
  - AccessDenied sayfası
- **Veritabanı**
  - ApplicationUser modeline `Departman` alanı (string?, nullable)
  - Migrations uygulandı

### 🔄 Devam Eden (Kişi 2):
- **Faz 2**: Model tasarımı (Ticket, TicketReply, Category)
- Müşteri ticket oluşturma/listeleme UI'ları
- TicketsController implementasyonu

### 📝 Bekleyen (Kişi 3):
- **Faz 3**: Support Agent dashboard + ticket yönetimi
- UI final tasarımı (layout, CSS)
- SupportController implementasyonu

## İletişim

Sorular için GitHub Issues kullanın veya takım leaderin ile iletişime geçin.
