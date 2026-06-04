using HelpDesk.Models;

namespace HelpDesk.ViewModels
{
    public class TicketDetailsViewModel
    {
        public Ticket Ticket { get; set; } = null!;
        public List<TicketReply> Yanitlar { get; set; } = new();
        public TicketReplyViewModel YeniYanit { get; set; } = new();
    }
}
