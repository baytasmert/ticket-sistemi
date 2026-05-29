using System.ComponentModel.DataAnnotations;

namespace HelpDesk.ViewModels
{
    public class TicketReplyViewModel
    {
        public int TicketId { get; set; }

        [Required(ErrorMessage = "Mesaj zorunludur.")]
        [StringLength(5000, ErrorMessage = "Mesaj en fazla 5000 karakter olmalıdır.")]
        [Display(Name = "Yanıtınız")]
        public string Mesaj { get; set; } = string.Empty;
    }
}
