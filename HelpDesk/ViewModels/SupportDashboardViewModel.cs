using HelpDesk.Models;

namespace HelpDesk.ViewModels
{
    public class KategoriSayisi
    {
        public string Kategori { get; set; } = string.Empty;
        public int Sayi { get; set; }
    }

    public class SupportDashboardViewModel
    {
        public int ToplamTalep { get; set; }
        public int AcikTalep { get; set; }
        public int IslemdeTalep { get; set; }
        public int CozulduTalep { get; set; }
        public int KapatildiTalep { get; set; }
        public int BanaAtanan { get; set; }
        public int AtanmamisTalep { get; set; }
        public List<KategoriSayisi> KategoriDagilimi { get; set; } = new();
        public List<Ticket> SonTalepler { get; set; } = new();
    }
}
