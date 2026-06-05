using HelpDesk.Helpers;
using HelpDesk.Models;
using HelpDesk.Services;
using Xunit;

namespace HelpDesk.Tests;

/// <summary>
/// Saf birim testleri: HTTP yığını olmadan iş kurallarını (hız sınırlama,
/// SLA/gecikme, rozet renkleri, göreceli zaman) doğrular. Servis/yardımcı
/// katmanına taşındıkları için artık izole test edilebiliyorlar.
/// </summary>
public class LoginThrottleTests
{
    [Fact]
    public void MaxDenemedenSonra_Kilitlenir()
    {
        var t = new LoginThrottle(maxAttempts: 3, lockoutMinutes: 15);

        Assert.Null(t.GetLockoutRemaining("ip"));
        t.RegisterFailure("ip");
        t.RegisterFailure("ip");
        Assert.Null(t.GetLockoutRemaining("ip")); // 2 < 3 → kilit yok

        t.RegisterFailure("ip"); // 3. başarısızlık → kilit
        Assert.NotNull(t.GetLockoutRemaining("ip"));
    }

    [Fact]
    public void Reset_KilidiKaldirir()
    {
        var t = new LoginThrottle(maxAttempts: 2, lockoutMinutes: 15);
        t.RegisterFailure("ip");
        t.RegisterFailure("ip");
        Assert.NotNull(t.GetLockoutRemaining("ip"));

        t.Reset("ip");
        Assert.Null(t.GetLockoutRemaining("ip"));
    }

    [Fact]
    public void FarkliIpler_BirbiriniEtkilemez()
    {
        var t = new LoginThrottle(maxAttempts: 1, lockoutMinutes: 15);
        t.RegisterFailure("ip-a");

        Assert.NotNull(t.GetLockoutRemaining("ip-a"));
        Assert.Null(t.GetLockoutRemaining("ip-b"));
    }
}

public class TicketViewHelpersTests
{
    [Theory]
    [InlineData(TicketDurumu.Açık, "bg-primary")]
    [InlineData(TicketDurumu.İşlemde, "bg-warning text-dark")]
    [InlineData(TicketDurumu.Çözüldü, "bg-success")]
    [InlineData(TicketDurumu.Kapatıldı, "bg-secondary")]
    public void DurumClass_DogruRenkVerir(TicketDurumu durum, string beklenen)
    {
        Assert.Equal(beklenen, TicketViewHelpers.DurumClass(durum));
    }

    [Fact]
    public void Gecikmis_KritikVeEskiAcikTalep_True()
    {
        // Kritik talebin SLA süresi 4 saat; 10 saat önce açılmışsa gecikmiştir.
        var t = new Ticket
        {
            Oncelik = TicketOnceligi.Kritik,
            Durum = TicketDurumu.Açık,
            OlusturmaTarihi = DateTime.Now.AddHours(-10)
        };
        Assert.True(TicketViewHelpers.Gecikmis(t));
    }

    [Fact]
    public void Gecikmis_YeniTalep_False()
    {
        var t = new Ticket
        {
            Oncelik = TicketOnceligi.Kritik,
            Durum = TicketDurumu.Açık,
            OlusturmaTarihi = DateTime.Now
        };
        Assert.False(TicketViewHelpers.Gecikmis(t));
    }

    [Fact]
    public void Gecikmis_KapatilmisTalep_HerZaman_False()
    {
        // Kapatılmış/çözülmüş talepler gecikmiş sayılmaz.
        var t = new Ticket
        {
            Oncelik = TicketOnceligi.Kritik,
            Durum = TicketDurumu.Kapatıldı,
            OlusturmaTarihi = DateTime.Now.AddDays(-100)
        };
        Assert.False(TicketViewHelpers.Gecikmis(t));
    }

    [Fact]
    public void SlaSure_OncelikArttikca_Kisalir()
    {
        Assert.True(TicketViewHelpers.SlaSure(TicketOnceligi.Kritik)
                  < TicketViewHelpers.SlaSure(TicketOnceligi.Düşük));
    }

    [Fact]
    public void GoreceliZaman_DakikayiDogruBicimler()
    {
        Assert.Equal("5 dakika önce", TicketViewHelpers.GoreceliZaman(DateTime.Now.AddMinutes(-5)));
    }
}
