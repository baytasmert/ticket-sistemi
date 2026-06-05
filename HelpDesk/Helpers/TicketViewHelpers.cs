using HelpDesk.Models;
using Microsoft.AspNetCore.Html;

namespace HelpDesk.Helpers
{
    /// <summary>
    /// View'larda talep durum/öncelik rozetleri, göreceli zaman ve SLA (hedef
    /// süre) gösterimi için ortak yardımcılar. Önceden bu mantık (özellikle
    /// rozet renkleri) 6 ayrı view'da kopyalanmıştı; tek yere toplandı.
    /// </summary>
    public static class TicketViewHelpers
    {
        // ── Rozet (badge) CSS sınıfları ──

        public static string DurumClass(TicketDurumu durum) => durum switch
        {
            TicketDurumu.Açık => "bg-primary",
            TicketDurumu.İşlemde => "bg-warning text-dark",
            TicketDurumu.Çözüldü => "bg-success",
            TicketDurumu.Kapatıldı => "bg-secondary",
            _ => "bg-secondary"
        };

        public static string OncelikClass(TicketOnceligi oncelik) => oncelik switch
        {
            TicketOnceligi.Düşük => "bg-info text-dark",
            TicketOnceligi.Orta => "bg-secondary",
            TicketOnceligi.Yüksek => "bg-warning text-dark",
            TicketOnceligi.Kritik => "bg-danger",
            _ => "bg-secondary"
        };

        /// <summary>Hazır durum rozeti (&lt;span class="badge ..."&gt;).</summary>
        public static IHtmlContent DurumRozet(TicketDurumu durum) =>
            Rozet(DurumClass(durum), durum.ToString());

        /// <summary>Hazır öncelik rozeti.</summary>
        public static IHtmlContent OncelikRozet(TicketOnceligi oncelik) =>
            Rozet(OncelikClass(oncelik), oncelik.ToString());

        private static IHtmlContent Rozet(string cssClass, string metin) =>
            // Metin sabit enum adlarından gelir (kullanıcı girdisi değil) → güvenli.
            new HtmlString($"<span class=\"badge {cssClass}\">{System.Net.WebUtility.HtmlEncode(metin)}</span>");

        // ── Göreceli zaman ──

        public static string GoreceliZaman(DateTime tarih)
        {
            var fark = DateTime.Now - tarih;
            if (fark.TotalSeconds < 0) return "az sonra";
            if (fark.TotalSeconds < 60) return "az önce";
            if (fark.TotalMinutes < 60) return $"{(int)fark.TotalMinutes} dakika önce";
            if (fark.TotalHours < 24) return $"{(int)fark.TotalHours} saat önce";
            if (fark.TotalDays < 30) return $"{(int)fark.TotalDays} gün önce";
            if (fark.TotalDays < 365) return $"{(int)(fark.TotalDays / 30)} ay önce";
            return $"{(int)(fark.TotalDays / 365)} yıl önce";
        }

        // ── SLA / hedef çözüm süresi ──
        // Önceliğe göre hedef yanıt/çözüm süresi. Açık/İşlemde talepler bu süreyi
        // aşarsa "gecikmiş" sayılır ve listede vurgulanır.

        public static TimeSpan SlaSure(TicketOnceligi oncelik) => oncelik switch
        {
            TicketOnceligi.Kritik => TimeSpan.FromHours(4),
            TicketOnceligi.Yüksek => TimeSpan.FromHours(24),
            TicketOnceligi.Orta => TimeSpan.FromHours(72),
            TicketOnceligi.Düşük => TimeSpan.FromHours(168),
            _ => TimeSpan.FromHours(72)
        };

        public static DateTime SonTarih(Ticket t) => t.OlusturmaTarihi + SlaSure(t.Oncelik);

        // Yalnızca henüz çözülmemiş/kapanmamış talepler gecikebilir.
        private static bool AcikDurum(TicketDurumu d) =>
            d == TicketDurumu.Açık || d == TicketDurumu.İşlemde;

        public static bool Gecikmis(Ticket t) =>
            AcikDurum(t.Durum) && DateTime.Now > SonTarih(t);

        /// <summary>SLA durumunu okunur metne çevirir (liste/detayda kullanılır).</summary>
        public static string SlaMetni(Ticket t)
        {
            if (!AcikDurum(t.Durum)) return "Tamamlandı";

            var kalan = SonTarih(t) - DateTime.Now;
            if (kalan <= TimeSpan.Zero)
            {
                var gecikme = -kalan;
                return gecikme.TotalDays >= 1
                    ? $"{(int)gecikme.TotalDays} gün gecikti"
                    : $"{(int)gecikme.TotalHours} saat gecikti";
            }
            return kalan.TotalDays >= 1
                ? $"{(int)kalan.TotalDays} gün kaldı"
                : $"{(int)kalan.TotalHours} saat kaldı";
        }

        // ── İşlem geçmişi (timeline) görselleştirme ──

        public static string IslemIkon(TicketIslemTuru tur) => tur switch
        {
            TicketIslemTuru.Olusturuldu => "bi-plus-circle",
            TicketIslemTuru.DurumDegisti => "bi-arrow-repeat",
            TicketIslemTuru.Atandi => "bi-person-check",
            TicketIslemTuru.YanitEklendi => "bi-chat-left-text",
            TicketIslemTuru.DahiliNot => "bi-sticky",
            TicketIslemTuru.Kapatildi => "bi-check2-circle",
            TicketIslemTuru.YenidenAcildi => "bi-arrow-counterclockwise",
            _ => "bi-dot"
        };

        public static string IslemRenk(TicketIslemTuru tur) => tur switch
        {
            TicketIslemTuru.Olusturuldu => "text-primary",
            TicketIslemTuru.DurumDegisti => "text-warning",
            TicketIslemTuru.Atandi => "text-info",
            TicketIslemTuru.YanitEklendi => "text-success",
            TicketIslemTuru.DahiliNot => "text-secondary",
            TicketIslemTuru.Kapatildi => "text-secondary",
            TicketIslemTuru.YenidenAcildi => "text-primary",
            _ => "text-muted"
        };
    }
}
