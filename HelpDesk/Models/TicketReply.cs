using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDesk.Models
{
    public class TicketReply
    {
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        [Required]
        public string YazarId { get; set; } = string.Empty;
        [ForeignKey("YazarId")]
        public ApplicationUser? Yazar { get; set; }

        [Required(ErrorMessage = "Mesaj zorunludur.")]
        [StringLength(5000, ErrorMessage = "Mesaj en fazla 5000 karakter olmalıdır.")]
        [Display(Name = "Mesaj")]
        public string Mesaj { get; set; } = string.Empty;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    }
}
