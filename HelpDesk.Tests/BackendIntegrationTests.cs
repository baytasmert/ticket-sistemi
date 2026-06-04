using System.Net;
using System.Text.RegularExpressions;
using Xunit;

namespace HelpDesk.Tests;

/// <summary>
/// Backend (controller + yetkilendirme + veri akışı) entegrasyon testleri.
/// Gerçek HTTP istekleri TestServer üzerinden gider; migration ve seed çalışır.
/// </summary>
public class BackendIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public BackendIntegrationTests(CustomWebApplicationFactory factory) => _factory = factory;

    // Her test için benzersiz e-posta üretir (test izolasyonu).
    private static string UniqueEmail(string prefix) => $"{prefix}_{Guid.NewGuid():N}@test.com";

    [Theory]
    [InlineData("/Tickets")]
    [InlineData("/Admin")]
    [InlineData("/Admin/Categories")]
    [InlineData("/Support/Dashboard")]
    [InlineData("/Account/Profile")]
    public async Task KorumaliRotalar_GirisYapilmamissa_LoginYonlendirir(string url)
    {
        var client = TestHelpers.CreateClient(_factory);

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Profile_GirisYapilmissa_200Doner()
    {
        // REGRESYON: eskiden Views/Account/Profile.cshtml eksik olduğu için 500 dönüyordu.
        var client = TestHelpers.CreateClient(_factory);
        await TestHelpers.RegisterCustomerAsync(client, UniqueEmail("profil"));

        var response = await client.GetAsync("/Account/Profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Profilim", html);
    }

    [Fact]
    public async Task SeedAdmin_StaffLogin_BasariliGiris()
    {
        var client = TestHelpers.CreateClient(_factory);

        var response = await TestHelpers.StaffLoginAsync(client, "admin@helpdesk.com", "Admin123!");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Support/Dashboard", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task SeedSupport_StaffLogin_BasariliGiris()
    {
        // REGRESYON: support@helpdesk.com hesabı seed'e sonradan eklendi.
        var client = TestHelpers.CreateClient(_factory);

        var response = await TestHelpers.StaffLoginAsync(client, "support@helpdesk.com", "Support123!");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Support/Dashboard", response.Headers.Location?.OriginalString);

        // Girişten sonra destek paneline erişebilmeli.
        var dashboard = await client.GetAsync("/Support/Dashboard");
        Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);
    }

    [Fact]
    public async Task MusteriKaydi_SonrasiTicketSayfasinaErisebilir()
    {
        var client = TestHelpers.CreateClient(_factory);
        await TestHelpers.RegisterCustomerAsync(client, UniqueEmail("musteri"));

        var response = await client.GetAsync("/Tickets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MusteriLogin_PersonelHesabiniReddeder()
    {
        // Admin/SupportAgent müşteri girişini kullanamaz; /staff/login'e yönlendirilir.
        var client = TestHelpers.CreateClient(_factory);

        var response = await TestHelpers.PostFormAsync(client, "/Account/Login", new()
        {
            ["Email"] = "admin@helpdesk.com",
            ["Password"] = "Admin123!",
            ["RememberMe"] = "false"
        });

        // Hata mesajıyla aynı sayfada kalır (yönlendirme yok).
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("/staff/login", html);
    }

    [Fact]
    public async Task Musteri_TicketOlustur_VeKapat_DurumKapatilirYanitFormuGizlenir()
    {
        // REGRESYON: müşteri için "Talebi Kapat" (Close) aksiyonu sonradan eklendi.
        var client = TestHelpers.CreateClient(_factory);
        await TestHelpers.RegisterCustomerAsync(client, UniqueEmail("kapat"));

        // Ticket oluştur (seed kategorilerinden Id=1).
        var create = await TestHelpers.PostFormAsync(client, "/Tickets/Create", new()
        {
            ["CategoryId"] = "1",
            ["Baslik"] = "Otomatik test talebi",
            ["Aciklama"] = "Bu bir test açıklamasıdır.",
            ["Oncelik"] = "1"
        });
        Assert.Equal(HttpStatusCode.Redirect, create.StatusCode);

        // Müşterinin yeni kaydı olduğu için listede sadece kendi talebi var; id'yi çıkar.
        var indexHtml = await (await client.GetAsync("/Tickets")).Content.ReadAsStringAsync();
        var idMatch = Regex.Match(indexHtml, @"/Tickets/Details/(\d+)");
        Assert.True(idMatch.Success, "Oluşturulan talebin id'si listede bulunamadı.");
        var ticketId = idMatch.Groups[1].Value;

        // Kapatmadan önce: detayda "Talebi Kapat" butonu olmalı.
        var beforeHtml = await (await client.GetAsync($"/Tickets/Details/{ticketId}")).Content.ReadAsStringAsync();
        Assert.Contains("Talebi Kapat", beforeHtml);

        // Talebi kapat (Close yalnızca-POST; token'ı Details sayfasından al).
        var close = await TestHelpers.PostFormAsync(client, "/Tickets/Close", new()
        {
            ["id"] = ticketId
        }, tokenUrl: $"/Tickets/Details/{ticketId}");
        Assert.Equal(HttpStatusCode.Redirect, close.StatusCode);

        // Kapattıktan sonra: kapanış bildirimi gösterilir, yanıt formu gizlenir.
        var afterHtml = await (await client.GetAsync($"/Tickets/Details/{ticketId}")).Content.ReadAsStringAsync();
        Assert.Contains("kapatılmıştır", afterHtml);
        Assert.DoesNotContain("Talebi Kapat", afterHtml);
    }

    [Fact]
    public async Task AdminKullaniciSil_TalebiOlanKullaniciyiEngeller()
    {
        // REGRESYON: DeleteUser artık bağlı talebi olan kullanıcıyı silmeyi engelliyor.
        var email = UniqueEmail("silinemez");

        // 1) Müşteri kaydı + bir talep oluştur.
        var customerClient = TestHelpers.CreateClient(_factory);
        await TestHelpers.RegisterCustomerAsync(customerClient, email);
        await TestHelpers.PostFormAsync(customerClient, "/Tickets/Create", new()
        {
            ["CategoryId"] = "1",
            ["Baslik"] = "Silme engeli testi",
            ["Aciklama"] = "Bağlı talep oluşturuldu.",
            ["Oncelik"] = "1"
        });

        // 2) Admin olarak giriş yap, kullanıcı listesinden bu müşterinin id'sini bul.
        var adminClient = TestHelpers.CreateClient(_factory);
        await TestHelpers.StaffLoginAsync(adminClient, "admin@helpdesk.com", "Admin123!");

        var usersHtml = await (await adminClient.GetAsync("/Admin")).Content.ReadAsStringAsync();
        var userId = ExtractUserIdForEmail(usersHtml, email);
        Assert.NotNull(userId);

        // 3) Silmeyi dene -> engellenmeli, kullanıcı hâlâ listede olmalı.
        //    DeleteUser yalnızca-POST; token'ı /Admin listesinden al.
        var delete = await TestHelpers.PostFormAsync(adminClient, "/Admin/DeleteUser", new()
        {
            ["userId"] = userId!
        }, tokenUrl: "/Admin");
        Assert.Equal(HttpStatusCode.Redirect, delete.StatusCode);

        var usersAfter = await (await adminClient.GetAsync("/Admin")).Content.ReadAsStringAsync();
        Assert.Contains(email, usersAfter);
    }

    // Admin kullanıcı listesi HTML'inden, verilen e-postaya ait satırdaki GUID userId'yi çıkarır.
    private static string? ExtractUserIdForEmail(string html, string email)
    {
        foreach (var row in Regex.Split(html, "(?=<tr)"))
        {
            if (row.Contains(email))
            {
                var m = Regex.Match(row, @"[Uu]serId=([a-f0-9\-]{36})");
                if (m.Success) return m.Groups[1].Value;
            }
        }
        return null;
    }
}
