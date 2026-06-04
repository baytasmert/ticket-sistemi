# HelpDesk — Veritabanı Şeması

**Motor:** SQLite · **Yaklaşım:** Entity Framework Core (Code-First) · **Üyelik:** ASP.NET Core Identity

Aşağıdaki diyagram GitHub'da ve VS Code'da (Mermaid eklentisiyle) görsel olarak render olur.

## ER Diyagramı

```mermaid
erDiagram
    AspNetUsers ||--o{ Tickets : "açar (MusteriId)"
    AspNetUsers |o--o{ Tickets : "atanır (AtananAjanId)"
    AspNetUsers ||--o{ TicketReplies : "yazar (YazarId)"
    Categories  ||--o{ Tickets : "içerir (CategoryId)"
    Tickets     ||--o{ TicketReplies : "sahip (TicketId)"
    AspNetUsers }o--o{ AspNetRoles : "AspNetUserRoles üzerinden"

    AspNetUsers {
        string Id PK "GUID"
        string Email
        string PasswordHash "hash'li"
        string AdSoyad
        string Telefon "nullable"
        string Departman "nullable, personel için"
        bool   AktifMi
    }

    Categories {
        int    Id PK
        string Ad
        bool   AktifMi
    }

    Tickets {
        int      Id PK
        string   Baslik
        string   Aciklama
        int      Durum "0=Açık,1=İşlemde,2=Çözüldü,3=Kapatıldı"
        int      Oncelik "0=Düşük,1=Orta,2=Yüksek,3=Kritik"
        int      CategoryId FK
        string   MusteriId FK
        string   AtananAjanId FK "nullable"
        datetime OlusturmaTarihi
        datetime GuncellenmeTarihi
    }

    TicketReplies {
        int      Id PK
        int      TicketId FK
        string   YazarId FK
        string   Mesaj
        datetime OlusturmaTarihi
    }

    Feedbacks {
        int      Id PK
        string   Kategori
        string   Mesaj
        string   Email "nullable"
        datetime CreatedAt
        bool     Okundu
    }
```

> Not: `Feedbacks` hiçbir tabloya bağlı değildir (bağımsız tablo) — üyelik gerektirmeyen genel geri bildirim formu içindir.

## Silme davranışları (Foreign Key `ON DELETE`)

| İlişki | Davranış | Anlamı |
|---|---|---|
| Ticket → Müşteri (MusteriId) | **RESTRICT** | Talebi olan müşteri silinemez |
| Ticket → Atanan Ajan (AtananAjanId) | **SET NULL** | Ajan silinirse talep "atanmamış" olur |
| Ticket → Kategori (CategoryId) | **CASCADE** | (DB) kategori silinirse talepleri de silinir — ancak uygulama, talebi olan kategorinin silinmesini engeller |
| TicketReply → Talep (TicketId) | **CASCADE** | Talep silinirse yanıtları da silinir |
| TicketReply → Yazar (YazarId) | **RESTRICT** | Yanıt yazan kullanıcı silinemez |

## Indexler
EF Core tüm yabancı anahtarlar için otomatik index oluşturur:
`IX_Tickets_CategoryId`, `IX_Tickets_MusteriId`, `IX_Tickets_AtananAjanId`,
`IX_TicketReplies_TicketId`, `IX_TicketReplies_YazarId`.

## Tablolar
**Domain tabloları (bizim):** `Tickets`, `TicketReplies`, `Categories`, `Feedbacks`
**Identity tabloları (otomatik):** `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetRoleClaims`, `AspNetUserLogins`, `AspNetUserTokens`

## İlk veriler (Seed)
DB ilk oluşturulduğunda otomatik eklenir:
- **Roller:** Admin, SupportAgent, Customer
- **Admin:** admin@helpdesk.com / Admin123!
- **Support:** support@helpdesk.com / Support123! (Departman: Teknik Destek)
- **Kategoriler:** Teknik Sorun, Fatura & Ödeme, Hesap & Erişim, Genel Bilgi Talebi, Diğer
