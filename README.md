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

## Varsayılan Admin Hesabı

- **Email**: admin@helpdesk.com
- **Şifre**: Admin123!
- **Rol**: Admin

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

### Faz 1 - Auth + Admin Paneli (Kişi 1)
- [ ] RegisterViewModel, LoginViewModel
- [ ] AccountController doldurma
- [ ] Login/Register/AccessDenied view'ları
- [ ] AdminController doldurma (kullanıcı yönetimi)
- [ ] Admin dashboard view'ları
- [ ] Navbar dinamik menü

### Faz 2 - Model Tasarımı + Müşteri Tarafı (Kişi 2)
- [ ] Ticket, TicketReply, Category modelleri
- [ ] DbContext'e DbSet'leri ekleme
- [ ] Ticket Controller ve view'ları
- [ ] Müşteri ticket listeleme/oluşturma

### Faz 3 - Destek Ekibi Tarafı (Kişi 3)
- [ ] SupportController doldurma
- [ ] Support dashboard
- [ ] Ticket filtreleme ve yönetimi

## Kontribütörler

- **Kişi 1**: İskelet + Auth + Admin
- **Kişi 2**: Model Tasarımı + Müşteri Tarafı
- **Kişi 3**: UI + Destek Tarafı
- **Kişi 4**: TBD

## Sorunlar & Notlar

- SQLite `helpdesk.db` dosyası proje root'unda otomatik oluşturulur
- İlk run'da veritabanı otomatik migrate edilir
- Roles ve admin kullanıcısı otomatik seed edilir

## İletişim

Sorular için GitHub Issues kullanın veya takım leaderin ile iletişime geçin.
