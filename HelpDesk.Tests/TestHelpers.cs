using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HelpDesk.Tests;

/// <summary>
/// Entegrasyon testleri için ortak yardımcılar: antiforgery token çıkarma,
/// form POST etme ve hazır giriş akışları. HttpClient çerezleri koruduğu için
/// (HandleCookies=true) aynı client üzerinde oturum devam eder.
/// </summary>
public static class TestHelpers
{
    /// <summary>Yönlendirmeleri takip etmeyen, çerez tutan bir client üretir.</summary>
    public static HttpClient CreateClient(CustomWebApplicationFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

    /// <summary>Bir HTML sayfasından __RequestVerificationToken değerini çıkarır.</summary>
    public static string ExtractAntiForgeryToken(string html)
    {
        var match = Regex.Match(
            html,
            @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");
        if (!match.Success)
            throw new InvalidOperationException("Antiforgery token sayfada bulunamadı.");
        return match.Groups[1].Value;
    }

    /// <summary>
    /// Token alıp verilen alanlarla <paramref name="url"/> adresine POST eder.
    /// Yalnızca-POST uçları (örn. /Tickets/Close) için token, formu barındıran
    /// farklı bir sayfadan <paramref name="tokenUrl"/> ile alınabilir.
    /// </summary>
    public static async Task<HttpResponseMessage> PostFormAsync(
        HttpClient client, string url, Dictionary<string, string> fields, string? tokenUrl = null)
    {
        var getResponse = await client.GetAsync(tokenUrl ?? url);
        var html = await getResponse.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(html);

        var data = new Dictionary<string, string>(fields)
        {
            ["__RequestVerificationToken"] = token
        };

        return await client.PostAsync(url, new FormUrlEncodedContent(data));
    }

    /// <summary>Yeni bir müşteri kaydı yapar (kayıt sonrası otomatik giriş yapılır).</summary>
    public static async Task RegisterCustomerAsync(
        HttpClient client, string email, string password = "Test123!")
    {
        var response = await PostFormAsync(client, "/Account/Register", new()
        {
            ["AdSoyad"] = "Test Kullanici",
            ["Email"] = email,
            ["Telefon"] = "5551234567",
            ["Password"] = password,
            ["ConfirmPassword"] = password
        });

        // Başarılı kayıt ana sayfaya yönlendirir.
        if (response.StatusCode != HttpStatusCode.Redirect)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Kayıt başarısız ({(int)response.StatusCode}). Yanıt: {body[..Math.Min(body.Length, 500)]}");
        }
    }

    /// <summary>Personel girişi (/staff/login) yapar.</summary>
    public static async Task<HttpResponseMessage> StaffLoginAsync(
        HttpClient client, string email, string password)
    {
        return await PostFormAsync(client, "/staff/login", new()
        {
            ["Email"] = email,
            ["Password"] = password,
            ["RememberMe"] = "false"
        });
    }
}
