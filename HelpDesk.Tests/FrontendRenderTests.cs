using System.Net;
using Xunit;

namespace HelpDesk.Tests;

/// <summary>
/// Frontend (Razor view render) testleri. Sunucu tarafında üretilen HTML'in
/// beklenen öğeleri (form alanları, navigasyon, butonlar, durum metinleri)
/// içerdiğini doğrular. Böylece eksik view veya bozuk sayfa "sürpriz" olmaz.
/// </summary>
public class FrontendRenderTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public FrontendRenderTests(CustomWebApplicationFactory factory) => _factory = factory;

    private static string UniqueEmail(string prefix) => $"{prefix}_{Guid.NewGuid():N}@test.com";

    private async Task<string> GetHtmlAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        // Razor, dinamik (@) çıktıdaki Türkçe karakterleri HTML entity'ye çevirir
        // (örn. "İyileştirme" -> "&#x130;yile&#x15F;tirme"). İçerik karşılaştırmasının
        // kodlamadan bağımsız olması için yanıtı decode ediyoruz.
        return WebUtility.HtmlDecode(html);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Account/Login")]
    [InlineData("/Account/Register")]
    [InlineData("/staff/login")]
    public async Task GenelSayfalar_200DonerVeMarkaBasligiIcerir(string url)
    {
        var client = TestHelpers.CreateClient(_factory);

        var html = await GetHtmlAsync(client, url);

        Assert.Contains("HelpDesk", html);
    }

    [Fact]
    public async Task LoginSayfasi_EmailVeSifreAlanlariIcerir()
    {
        var client = TestHelpers.CreateClient(_factory);

        var html = await GetHtmlAsync(client, "/Account/Login");

        Assert.Contains("name=\"Email\"", html);
        Assert.Contains("name=\"Password\"", html);
        Assert.Contains("__RequestVerificationToken", html);
    }

    [Fact]
    public async Task RegisterSayfasi_TumZorunluAlanlariIcerir()
    {
        var client = TestHelpers.CreateClient(_factory);

        var html = await GetHtmlAsync(client, "/Account/Register");

        Assert.Contains("name=\"AdSoyad\"", html);
        Assert.Contains("name=\"Email\"", html);
        Assert.Contains("name=\"Password\"", html);
        Assert.Contains("name=\"ConfirmPassword\"", html);
    }

    [Fact]
    public async Task AnaSayfa_GirisYapilmamissa_GirisVeKayitLinkleriGosterir()
    {
        var client = TestHelpers.CreateClient(_factory);

        var html = await GetHtmlAsync(client, "/");

        Assert.Contains("Giriş Yap", html);
        Assert.Contains("Kayıt Ol", html);
        Assert.Contains("Personel Girişi", html);
    }

    [Fact]
    public async Task Navbar_MusteriGirisindeTaleplerimGosterir()
    {
        var client = TestHelpers.CreateClient(_factory);
        await TestHelpers.RegisterCustomerAsync(client, UniqueEmail("nav"));

        var html = await GetHtmlAsync(client, "/");

        Assert.Contains("Taleplerim", html);
        Assert.Contains("Çıkış Yap", html);
    }

    [Fact]
    public async Task TicketOlusturSayfasi_KategoriVeOncelikSecimiIcerir()
    {
        var client = TestHelpers.CreateClient(_factory);
        await TestHelpers.RegisterCustomerAsync(client, UniqueEmail("create"));

        var html = await GetHtmlAsync(client, "/Tickets/Create");

        Assert.Contains("name=\"CategoryId\"", html);
        Assert.Contains("name=\"Baslik\"", html);
        Assert.Contains("name=\"Oncelik\"", html);
        // Seed kategorilerinden en az biri dropdown'da görünmeli.
        Assert.Contains("Teknik Sorun", html);
    }
}
