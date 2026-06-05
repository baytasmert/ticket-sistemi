using System.Net;
using System.Text.RegularExpressions;
using Xunit;

namespace HelpDesk.Tests;

/// <summary>
/// Yeni eklenen özelliklerin (dahili notlar, işlem geçmişi) uçtan uca
/// davranışını doğrulayan entegrasyon testleri.
/// </summary>
public class FeatureTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public FeatureTests(CustomWebApplicationFactory factory) => _factory = factory;

    private static string UniqueEmail(string prefix) => $"{prefix}_{Guid.NewGuid():N}@test.com";

    // Bir müşteri talebi oluşturup id'sini döndürür.
    private static async Task<string> CreateTicketAsync(HttpClient client, string baslik)
    {
        var create = await TestHelpers.PostFormAsync(client, "/Tickets/Create", new()
        {
            ["CategoryId"] = "1",
            ["Baslik"] = baslik,
            ["Aciklama"] = "Test açıklaması.",
            ["Oncelik"] = "1"
        });
        Assert.Equal(HttpStatusCode.Redirect, create.StatusCode);

        var indexHtml = await (await client.GetAsync("/Tickets")).Content.ReadAsStringAsync();
        var m = Regex.Match(indexHtml, @"/Tickets/Details/(\d+)");
        Assert.True(m.Success, "Oluşturulan talebin id'si bulunamadı.");
        return m.Groups[1].Value;
    }

    [Fact]
    public async Task DahiliNot_MusteriyeGizli_DestekGorur()
    {
        var marker = $"GIZLI_{Guid.NewGuid():N}";

        // 1) Müşteri talep oluşturur.
        var custClient = TestHelpers.CreateClient(_factory);
        await TestHelpers.RegisterCustomerAsync(custClient, UniqueEmail("dahili"));
        var ticketId = await CreateTicketAsync(custClient, "Dahili not testi");

        // 2) Destek girişi yapıp dahili not ekler.
        var supClient = TestHelpers.CreateClient(_factory);
        await TestHelpers.StaffLoginAsync(supClient, "support@helpdesk.com", "Support123!");
        var notReply = await TestHelpers.PostFormAsync(supClient, "/Support/AddReply", new()
        {
            ["ticketId"] = ticketId,
            ["mesaj"] = marker,
            ["dahili"] = "true"
        }, tokenUrl: $"/Support/Details/{ticketId}");
        Assert.Equal(HttpStatusCode.Redirect, notReply.StatusCode);

        // 3) Müşteri detayında dahili not GÖRÜNMEZ.
        var custHtml = await (await custClient.GetAsync($"/Tickets/Details/{ticketId}")).Content.ReadAsStringAsync();
        Assert.DoesNotContain(marker, custHtml);

        // 4) Destek detayında dahili not GÖRÜNÜR.
        var supHtml = await (await supClient.GetAsync($"/Support/Details/{ticketId}")).Content.ReadAsStringAsync();
        Assert.Contains(marker, supHtml);
    }

    [Fact]
    public async Task DestekNormalYaniti_MusteriyeGorunur()
    {
        var marker = $"ACIK_{Guid.NewGuid():N}";

        var custClient = TestHelpers.CreateClient(_factory);
        await TestHelpers.RegisterCustomerAsync(custClient, UniqueEmail("acik"));
        var ticketId = await CreateTicketAsync(custClient, "Açık yanıt testi");

        var supClient = TestHelpers.CreateClient(_factory);
        await TestHelpers.StaffLoginAsync(supClient, "support@helpdesk.com", "Support123!");
        await TestHelpers.PostFormAsync(supClient, "/Support/AddReply", new()
        {
            ["ticketId"] = ticketId,
            ["mesaj"] = marker
            // dahili gönderilmez → normal (herkese açık) yanıt
        }, tokenUrl: $"/Support/Details/{ticketId}");

        // Müşteri normal yanıtı görebilmeli.
        var custHtml = await (await custClient.GetAsync($"/Tickets/Details/{ticketId}")).Content.ReadAsStringAsync();
        Assert.Contains(marker, custHtml);
    }

    [Theory]
    [InlineData("/Support/Dashboard")]
    [InlineData("/Support")]
    [InlineData("/Support/AssignedToMe")]
    public async Task DestekPanelSayfalari_Render(string url)
    {
        // Yeniden tasarlanan destek panel sayfaları çalışma-zamanı hatası olmadan render olmalı.
        var client = TestHelpers.CreateClient(_factory);
        await TestHelpers.StaffLoginAsync(client, "support@helpdesk.com", "Support123!");

        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/Admin/Dashboard")]
    [InlineData("/Admin")]
    [InlineData("/Admin/AllTickets")]
    [InlineData("/Admin/Categories")]
    public async Task AdminPanelSayfalari_Render(string url)
    {
        var client = TestHelpers.CreateClient(_factory);
        await TestHelpers.StaffLoginAsync(client, "admin@helpdesk.com", "Admin123!");

        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TalepOlusturma_IslemGecmisineKaydedilir()
    {
        var custClient = TestHelpers.CreateClient(_factory);
        await TestHelpers.RegisterCustomerAsync(custClient, UniqueEmail("gecmis"));
        var ticketId = await CreateTicketAsync(custClient, "Geçmiş testi");

        var html = await (await custClient.GetAsync($"/Tickets/Details/{ticketId}")).Content.ReadAsStringAsync();
        var decoded = WebUtility.HtmlDecode(html);
        Assert.Contains("Talep oluşturuldu", decoded);
    }
}
