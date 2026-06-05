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

        // Dahili not: yalnızca destek ekibi görür, müşteriye gösterilmez.
        // Ekip üyeleri talep hakkında birbirlerine özel notlar bırakabilir.
        [Display(Name = "Dahili Not")]
        public bool DahiliMi { get; set; } = false;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    }
}
