using HelpDesk.Models;

namespace HelpDesk.ViewModels
{
    /// <summary>
    /// Admin gösterge panosu için GERÇEK verilerden hesaplanan metrikler.
    /// (Önceki panoda "API yanıt süresi" / "veritabanı durumu" gibi sabit kodlanmış
    /// sahte değerler vardı; bunlar gerçek talep analitikleriyle değiştirildi.)
    /// </summary>
    public class AdminDashboardViewModel
    {
        // Kullanıcılar
        public int ToplamKullanici { get; set; }
        public int ToplamMusteri { get; set; }
        public int ToplamDestek { get; set; }
        public int ToplamAdmin { get; set; }

        // Talep durum dağılımı
        public int ToplamTalep { get; set; }
        public int Acik { get; set; }
        public int Islemde { get; set; }
        public int Cozuldu { get; set; }
        public int Kapatildi { get; set; }
        public int Atanmamis { get; set; }
        public int Gecikmis { get; set; }

        // Aylık ve performans
        public int BuAyAcilan { get; set; }
        public int BuAyTamamlanan { get; set; }
        public double OrtalamaCozumSaati { get; set; }

        public List<KategoriSayisi> KategoriDagilimi { get; set; } = new();
        public List<Ticket> SonTalepler { get; set; } = new();

        // Açık + işlemdeki toplam (aktif iş yükü) — view kolaylığı için.
        public int AktifTalep => Acik + Islemde;

        // Çözüm oranı (%) — kapatılan/çözülen oranı.
        public int CozumOrani => ToplamTalep > 0
            ? (int)Math.Round((Cozuldu + Kapatildi) * 100.0 / ToplamTalep)
            : 0;

        // Ortalama çözüm süresinin okunur biçimi.
        public string OrtalamaCozumMetni
        {
            get
            {
                if (OrtalamaCozumSaati <= 0) return "—";
                if (OrtalamaCozumSaati < 1) return $"{(int)(OrtalamaCozumSaati * 60)} dk";
                if (OrtalamaCozumSaati < 24) return $"{OrtalamaCozumSaati:0.#} saat";
                return $"{OrtalamaCozumSaati / 24:0.#} gün";
            }
        }
    }
}
