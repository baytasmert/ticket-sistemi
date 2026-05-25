# HelpDesk Ekip Planlama Dokümantasyonu

## 📋 Genel Bakış

HelpDesk projesi 3 kişilik ekip tarafından 3 faz halinde geliştirilmektedir:
- **Kişi 1**: İskelet + Authentication + Admin Paneli (FER TAMAMLANDI)
- **Kişi 2**: Model Tasarımı + Müşteri Tarafı (DEVAM EDİYOR)
- **Kişi 3**: UI Tasarımı + Destek Ekibi Tarafı (PLANLAMA)

---

## 📊 Dashboard Mimarisi

Sistemde **iki farklı dashboard** vardır - amaçları ve içerikleri farklı:

| | **Admin Dashboard** | **Support Dashboard** |
|---|---|---|
| **URL** | `/Admin` (Kişi 1) | `/Support/Dashboard` (Kişi 3) |
| **Kim için** | Admin → Sistem yönetimi | Support Agent/Admin → Ticket yönetimi |
| **Amacı** | Sistem sağlığı, kullanıcı yönetimi | Günlük iş - bekleyen talepler |
| **İçerik** | Kullanıcı istatistikleri, geri bildirimler | Talep istatistikleri, bekleyen talepler |
| **Kartlar** | Toplam Kullanıcı, Admin, SupportAgent, Customer | Toplam Talep, Açık, Cevaplandi, Çözüldü, Kapalı, Bana Atanan |
| **Tablolar** | Kullanıcı listesi | Bekleyen talepler, Son talepler, Kategoriye göre dağılım |

---

## 🏗️ Kayıt Sistemi Tasarımı (Endüstri Standardı)

### Araştırma Bulguları
Zendesk, Freshdesk, osTicket, Hesk gibi profesyonel sistemlerin incelenmesi sonucunda:

| Sistem | Müşteri Kayıt | Staff Hesabı | Onay Akışı |
|---|---|---|---|
| **Zendesk** | Self-registration | Admin oluşturur | Müşteri: anında, Staff: admin tarafından |
| **Freshdesk** | Self-registration | Admin oluşturur | Müşteri: anında, Staff: anında |
| **osTicket** | Self-registration | Admin oluşturur | Müşteri: anında, Staff: anında |
| **Hesk** | Self-registration | Admin oluşturur | Müşteri: anında, Staff: anında |

**Sonuç**: Hiçbir sistem "staff başvurusu + admin onayı" modeli kullanmaz. Bu gereksiz karmaşıklık.

### HelpDesk'te Tasarlanan Model

```
Endpointler:
/                 → Müşteri portalı (public)
/staff/login      → Personel girişi (SupportAgent + Admin)
/Admin            → Admin paneli (hesap yönetimi)
```

| Kullanıcı Tipi | Kayıt Yöntemi | Rol Atanması | Durum |
|---|---|---|---|
| **Müşteri** | `/Account/Register` (self) | Customer (otomatik) | Anında aktif |
| **SupportAgent** | Admin `/Admin/CreateUser` | SupportAgent + Departman seçimi | Anında aktif |
| **Admin** | Seed'den veya mevcut admin tarafından | Admin | Anında aktif |

### Departman Sistemi
- **SupportAgent** hesapları oluşturulurken departman atanır
- Departmanlar: Teknik Destek, Müşteri Hizmetleri, Fatura & Ödeme, Genel
- Admin tarafından `/Admin/CreateUser` sayfasında seçilir
- `ApplicationUser.Departman` alanı (string?, nullable) sadece staff için dolu

---

## 👥 Takım Görevleri & Sorumluluklar

### Kişi 1 - İskelet + Auth + Admin ✅ TAMAMLANDI

#### Sorumluluğu Altında (Tamamlandı)
- [x] Proje iskeletinin oluşturulması
- [x] Entity Framework Core + SQLite kurulumu
- [x] ASP.NET Core Identity konfigürasyonu
- [x] Database migrations altyapısı
- [x] ApplicationUser modeli
- [x] AccountController (Register, Login, Logout, AccessDenied)
- [x] AdminController (Index, CreateUser, EditRole, ToggleActive, DeleteUser)
- [x] ViewModels: RegisterViewModel, LoginViewModel, CreateUserViewModel
- [x] Views: Login, Register, Admin paneli view'ları
- [x] Navbar role-based dinamik menü
- [x] StaffController (/staff/login endpoint)
- [x] Department field eklemesi (ApplicationUser.Departman)
- [x] Seed data (admin hesabı + roller)

#### Kişi 2 Bittikten Sonra Yapacağı
- [ ] Admin paneline **Kategori CRUD** ekleme
- [ ] Kategori yönetimi (Teknik Destek kategorileri: Teknik, Ödeme, Hesap, Genel)

---

### Kişi 2 - Model Tasarımı + Müşteri Tarafı 🔄 BAŞLANACAK

#### Sorumluluğu Altında

##### 1. Database Models & Enums
Dosyalar: `Models/Ticket.cs`, `Models/TicketReply.cs`, `Models/Category.cs`

```csharp
public class Category
{
    public int Id { get; set; }
    public string Ad { get; set; }  // "Teknik Destek", "Ödeme", etc.
    public string? Aciklama { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public ICollection<Ticket> Tickets { get; set; }
}

public enum TicketStatus { Acik, Cevaplandi, Cozuldu, Kapandi }
public enum TicketPriority { Dusuk = 1, Orta = 2, Yuksek = 3 }

public class Ticket
{
    public int Id { get; set; }
    public string Baslik { get; set; }
    public string Aciklama { get; set; }
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    
    // Foreign Keys
    public int CategoryId { get; set; }
    public string CustomerId { get; set; }  // ApplicationUser.Id
    public string? AssignedAgentId { get; set; }  // SupportAgent/Admin (nullable)
    
    // Navigation
    public Category Category { get; set; }
    public ApplicationUser Customer { get; set; }
    public ApplicationUser? AssignedAgent { get; set; }
    public ICollection<TicketReply> Replies { get; set; }
}

public class TicketReply
{
    public int Id { get; set; }
    public string Mesaj { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Foreign Keys
    public int TicketId { get; set; }
    public string UserId { get; set; }  // ApplicationUser.Id
    
    // Navigation
    public Ticket Ticket { get; set; }
    public ApplicationUser User { get; set; }
}
```

**Enums konumu**: `Models/Ticket.cs` içinde namespace seviyesinde

##### 2. Database Context Güncellemesi
Dosya: `Data/ApplicationDbContext.cs`

```csharp
public DbSet<Category> Categories { get; set; }
public DbSet<Ticket> Tickets { get; set; }
public DbSet<TicketReply> TicketReplies { get; set; }

// OnModelCreating içinde:
// - Ticket → Category (1:N) FK constraint
// - Ticket → Customer (1:N) FK constraint
// - Ticket → AssignedAgent (1:N, nullable) FK constraint
// - TicketReply → Ticket (1:N) FK constraint
// - TicketReply → User (1:N) FK constraint
// - Cascade delete kuralları
```

##### 3. Seed Data
Dosya: `Data/SeedData.cs`

```csharp
// Program.cs içinde Configuration çağrıldığında:
SeedData.Initialize(context);

// Örnek kategoriler seed et:
// - Teknik Destek
// - Ödeme
// - Hesap Yönetimi
// - Genel Sorular
```

##### 4. Migration
```bash
dotnet ef migrations add AddTicketModels
dotnet ef database update
```

##### 5. Controllers
Dosya: `Controllers/TicketsController.cs`

```csharp
[Authorize(Roles = "Customer")]
public class TicketsController : Controller
{
    // Index: Müşterinin kendi ticket'larını listele (arama + filtre)
    // GET /Tickets
    public IActionResult Index(string search, string status)
    
    // Create: Yeni ticket oluştur sayfası
    // GET /Tickets/Create
    public IActionResult Create()
    
    // Create: Form submit
    // POST /Tickets/Create
    public async Task<IActionResult> Create(TicketCreateViewModel model)
    
    // Details: Ticket detayı + yanıtlar
    // GET /Tickets/{id}
    public async Task<IActionResult> Details(int id)
    
    // Reply: Ticket'a yanıt ekle
    // POST /Tickets/{ticketId}/Reply
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int ticketId, string mesaj)
    
    // Close: Müşteri ticket'ı kapatabilir
    // POST /Tickets/{id}/Close
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id)
}
```

##### 6. ViewModels
Dosya: `ViewModels/TicketViewModels.cs`

```csharp
public class TicketCreateViewModel
{
    [Required]
    public int CategoryId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Baslik { get; set; }
    
    [Required]
    [StringLength(5000)]
    public string Aciklama { get; set; }
    
    [Required]
    public TicketPriority Priority { get; set; }
}

public class TicketDetailViewModel
{
    public int Id { get; set; }
    public string Baslik { get; set; }
    public string Aciklama { get; set; }
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CategoryName { get; set; }
    public ICollection<TicketReplyViewModel> Replies { get; set; }
}

public class TicketListItemViewModel
{
    public int Id { get; set; }
    public string Baslik { get; set; }
    public string CategoryName { get; set; }
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TicketReplyViewModel
{
    public int Id { get; set; }
    public string Mesaj { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserName { get; set; }
    public string UserId { get; set; }
}
```

##### 7. Views
Dosyalar: `Views/Tickets/` klasörü

**Index.cshtml** - Ticket listesi
- Arama ve filtre (durum, kategori)
- Tablo: Başlık, Kategori, Durum (renkli rozet), Öncelik, Oluşturma tarihi
- "Yeni Ticket" butonu
- Sayfalandırma (opsiyonel)

**Create.cshtml** - Yeni ticket formu
- Kategori dropdown
- Başlık input
- Açıklama textarea
- Öncelik select
- "Gönder" butonu

**Details.cshtml** - Ticket detayı
- Ticket bilgileri (başlık, açıklama, durum, kategori, öncelik)
- Yanıtlar listesi (tarih, kullanıcı, mesaj)
- Yeni yanıt formu (sadece müşteri kendi ticket'ına yazabilir)
- Ticket kapatma butonu (sadece müşteri, durum "Kapandi" değilse)

---

### Kişi 3 - UI + Destek Ekibi Tarafı 🔄 PLANLAMA

#### Sorumluluğu Altında

##### 1. Frontend (Kişi 2'yi beklemeden paralel yapılabilir)

**Layout & Styling**
Dosyalar: `Views/Shared/_Layout.cshtml`, `wwwroot/css/site.css`

Hedefler:
- Profesyonel, kurumsal görünüm
- Bootstrap 5 + custom CSS
- Responsive design (mobile-first)
- Durum rozetleri (açık=mavi, cevaplandi=sarı, çözüldü=yeşil, kapalı=gri)
- Öncelik rozetleri (düşük=mavi, orta=sarı, yüksek=kırmızı)
- Consistent spacing ve typography

**Ortak Components**
Dosya: `Views/Shared/_AlertPartial.cshtml`
- Toast/alert kısmı (başarı, hata, bilgi, uyarı)
- TempData mesajlarını göstermek için

##### 2. Support Agent Dashboard & Management (Kişi 2 sonrası)

**Controller**
Dosya: `Controllers/SupportController.cs`

```csharp
[Authorize(Roles = "SupportAgent, Admin")]
[Route("Support")]
public class SupportController : Controller
{
    // Dashboard: İstatistikler + bekleyen talepler
    // GET /Support/Dashboard
    public async Task<IActionResult> Dashboard()
    
    // Index: Filtreleme ile ticket listesi
    // GET /Support
    public async Task<IActionResult> Index(
        string search, 
        string status, 
        string category, 
        string priority,
        string assignedTo)
    
    // AssignedToMe: Bana atanan ticket'lar
    // GET /Support/AssignedToMe
    public async Task<IActionResult> AssignedToMe()
    
    // Assign: Ticket bana ata
    // POST /Support/Assign/{ticketId}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int ticketId)
    
    // Unassign: Ticket atamadan çık
    // POST /Support/Unassign/{ticketId}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unassign(int ticketId)
    
    // UpdateStatus: Ticket durumunu güncelle
    // POST /Support/UpdateStatus/{ticketId}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int ticketId, TicketStatus newStatus)
    
    // Reply: Ticket'a yanıt ekle
    // POST /Support/{ticketId}/Reply
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int ticketId, string mesaj)
}
```

**ViewModels**
Dosya: `ViewModels/SupportViewModels.cs`

```csharp
public class SupportDashboardViewModel
{
    public int ToplamTicket { get; set; }
    public int AcikTicket { get; set; }
    public int CevaplandiTicket { get; set; }
    public int CozuluTicket { get; set; }
    public int KapaliTicket { get; set; }
    public int BanaAtananTicket { get; set; }
    
    public ICollection<TicketListItemViewModel> BekleyenTalepler { get; set; }
}

public class TicketFilterViewModel
{
    public string Search { get; set; }
    public TicketStatus? Status { get; set; }
    public int? CategoryId { get; set; }
    public TicketPriority? Priority { get; set; }
    public string AssignedTo { get; set; }  // "BanaAtanan" veya "Atanmamis"
}
```

**Views**
Dosyalar: `Views/Support/` klasörü

**Dashboard.cshtml** - Support Ekibi Panosu
- **İstatistik Kartları** (toplam, açık, cevaplandi, çözüldü, kapalı, bana atanan)
- **Bekleyen Talepler Tablosu** (atanmamış + acil talepler - Yüksek öncelik, Açık status)
- **Son Eklenen Talepler** (son 5 talep)
- **Kategoriye Göre Dağılım** (mini chart veya tablo)
- **Amaç**: Destek ekibine hızlı bakış - neleri yapması gerekiyor, acil neler var

**Index.cshtml** - Talepleri Yönet
- Filtreleme UI (arama, durum, kategori, öncelik, atanma durumu)
- Ticket tablosu: Başlık, Müşteri, Kategori, Durum, Öncelik, Atanan, Oluşturma tarihi
- Eylem butonu: Atayı (assign), Atamadan çık (unassign), Detayları gör (details)
- Sayfalandırma
- **Amaç**: Tüm talepleri filtreleyip yönetmek

**AssignedToMe.cshtml** - Kişisel Taleplerim
- Bana atanan ticket'ların listesi
- Durum filtresine göre gruplandırılmış (Açık, Cevaplandi, Çözüldü, Kapalı)
- Her ticket için: Başlık, Müşteri, Kategori, Detayları gör
- **Amaç**: Sadece bana atanan talepleri hızlıca görmek

---

## 🔗 Bağımlılıklar & Koordinasyon Noktaları

### Kişi 1 → Kişi 2
- **Bekle**: ApplicationUser, Roller sistem hazır ✅
- **Başlat**: Ticket modelleri oluştur

### Kişi 2 → Kişi 1 (Ek iş)
- **Sonra**: Kişi 1, `/Admin` paneline kategori CRUD'u ekler

### Kişi 2 ↔ Kişi 3
- **Paralel**: Kişi 3 frontend tasarımını (layout, CSS) yapabilir
- **Ardışık**: Kişi 3, Kişi 2'nin view'larını final styling ile günceller

### Çakışma Riski (Koordine Et)
| Dosya | Kişi | Risk | Çözüm |
|---|---|---|---|
| `Data/ApplicationDbContext.cs` | Kişi 2 | DbSet ekleme | Branch'i çabuk merge et |
| `Data/SeedData.cs` | Kişi 2 | Kategori seed | Main'e merge hemen sonra pull |
| `Views/Shared/_Layout.cshtml` | Kişi 3 | Style override | Kişi 3, final tasarımda dikkat et |
| `wwwroot/css/site.css` | Kişi 3 | Global stiller | Modular CSS (BEM vb.) kullan |
| `Views/Shared/` | Kişi 1, 2, 3 | Partial overrides | Koordine yap |

---

## 📊 İş Akışı ve Timeline

```
Hafta 1-2:
├─ Kişi 1: Faz 0 + Faz 1 ✅ TAMAMLANDI
├─ Kişi 2: Faz 2'yi başla (Models + Database)
└─ Kişi 3: Layout + CSS tasarımına başla

Hafta 2-3:
├─ Kişi 2: TicketsController + Views tamamla
├─ Kişi 3: Frontend'i finalize et
└─ Kişi 1: Kategori CRUD'u admin paneline ekle

Hafta 3-4:
├─ Kişi 2: Faz 2 merge
├─ Kişi 3: SupportController + Views (Faz 3) tamamla
└─ Tümü: Integration testing & bug fixing
```

---

## 🧪 Test Hesapları ve Giriş Yolları

### Müşteri Portalı
- **URL**: `http://localhost:5000` veya `http://localhost:5000/`
- **Kayıt**: `/Account/Register` → Customer rolü otomatik
- **Giriş**: `/Account/Login`
- **Taleplerim**: `/Tickets` (Giriş sonrası visible)

### Personel Girişi
- **URL**: `http://localhost:5000/staff/login`
- Sadece Admin ve SupportAgent rolleri giriş yapabilir
- Başarılı giriş → `/Support/Dashboard` yönlendir

### Admin Paneli
- **URL**: `http://localhost:5000/Admin/Index`
- Sadece Admin rolüne erişim
- Yeni kullanıcı: `/Admin/CreateUser`
- Rol değişikliği: `/Admin/EditRole/{userId}`
- Kategori yönetimi: `/Admin/Categories` (Kişi 1'in ekleyeceği)

### Default Test Hesapları
```
Admin:
- Email: admin@helpdesk.com
- Şifre: Admin123!
- Giriş: /staff/login

SupportAgent:
- Email: support@helpdesk.com
- Şifre: Support123!
- Departman: Teknik Destek
- Giriş: /staff/login

Müşteri (self-register):
- Register at: /Account/Register
- Giriş: /Account/Login
```

---

## 📝 Commit Convention

Branch yapısı:
```
main (production)
├─ kisi1 (Kişi 1'in feature branch'i)
├─ kisi2 (Kişi 2'nin feature branch'i)
└─ kisi3 (Kişi 3'ün feature branch'i)
```

Commit mesajı formatı:
```
[Faz X] [Kişi Y] - Kısa açıklama

Örnek:
[Faz 2] [Kişi 2] - Ticket ve Category modelleri eklendi
[Faz 3] [Kişi 3] - Support Dashboard UI tamamlandı
```

---

## 📞 İletişim & Sorular

- **GitHub Issues**: Bug veya soru için issue aç
- **Pull Requests**: Feature branch'i main'e merge etmek için PR aç
- **Koordinasyon**: Çakışma riski gördüysen commit etmeden önce haberim ver

---

*Son güncelleme: 26 Mayıs 2026*
*Kişi 1 tarafından hazırlanmıştır*
