using HelpDesk.Models;

namespace HelpDesk.ViewModels
{
    public class TicketDetailsViewModel
    {
        public Ticket Ticket { get; set; } = null!;
        public List<TicketReply> Yanitlar { get; set; } = new();

        // Talep işlem geçmişi (zaman çizelgesi). Müşteri görünümünde dahili
        // kayıtlar zaten filtrelenmiş olarak gelir.
        public List<TicketHistory> Gecmis { get; set; } = new();

        public TicketReplyViewModel YeniYanit { get; set; } = new();
    }
}
