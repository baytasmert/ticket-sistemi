using HelpDesk.Models;
using HelpDesk.ViewModels;

namespace HelpDesk.Services
{
    /// <summary>
    /// Talep (ticket) iş mantığının tek yetkili yeri. Controller'lar yalnızca
    /// kimlik (kullanıcı id / rol) ve görüntüleme ile ilgilenir; sorgular, durum
    /// geçişleri, atama ve işlem geçmişi (TicketHistory) kaydı burada toplanır.
    /// Böylece mantık tek noktadan yönetilir ve birim test edilebilir.
    /// </summary>
    public interface ITicketService
    {
        // ── Müşteri akışı ──
        Task<List<Ticket>> GetMusteriTalepleriAsync(string musteriId, TicketDurumu? durum);
        Task<Dictionary<TicketDurumu, int>> GetMusteriDurumSayilariAsync(string musteriId);
        Task<TicketDetailsViewModel?> GetMusteriDetayAsync(int ticketId, string musteriId);
        Task<Ticket> OlusturAsync(string musteriId, TicketCreateViewModel model);
        Task<bool> MusteriYanitEkleAsync(int ticketId, string musteriId, string mesaj);
        /// <returns>null: talep bulunamadı · true: kapatıldı · false: zaten kapalıydı</returns>
        Task<bool?> MusteriKapatAsync(int ticketId, string musteriId);

        // ── Destek / Admin akışı ──
        Task<SupportDashboardViewModel> GetDashboardAsync(string agentId);
        Task<List<Ticket>> FiltreleAsync(string? durum, int? kategoriId, string? oncelik, string? arama);
        Task<List<Ticket>> GetAtananlarAsync(string agentId, TicketDurumu? durum);
        Task<TicketDetailsViewModel?> GetDestekDetayAsync(int ticketId);
        Task<bool> AtaAsync(int ticketId, string actorId, string? hedefAjanId);
        Task<bool> AjanYanitEkleAsync(int ticketId, string ajanId, string mesaj, bool dahili, TicketDurumu? yeniDurum);
        Task<bool> DurumGuncelleAsync(int ticketId, string actorId, TicketDurumu durum);

        // ── Ortak ──
        Task<List<Category>> GetAktifKategorilerAsync();
        Task<List<ApplicationUser>> GetDestekAjanlariAsync();
    }
}
