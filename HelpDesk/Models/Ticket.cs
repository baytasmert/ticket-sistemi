using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDesk.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur.")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olmalıdır.")]
        [Display(Name = "Başlık")]
        public string Baslik { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [StringLength(5000, ErrorMessage = "Açıklama en fazla 5000 karakter olmalıdır.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; } = string.Empty;

        [Display(Name = "Durum")]
        public TicketDurumu Durum { get; set; } = TicketDurumu.Açık;

        [Display(Name = "Öncelik")]
        public TicketOnceligi Oncelik { get; set; } = TicketOnceligi.Orta;

        [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required]
        public string MusteriId { get; set; } = string.Empty;
        [ForeignKey("MusteriId")]
        public ApplicationUser? Musteri { get; set; }

        public string? AtananAjanId { get; set; }
        [ForeignKey("AtananAjanId")]
        public ApplicationUser? AtananAjan { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public DateTime GuncellenmeTarihi { get; set; } = DateTime.Now;

        public ICollection<TicketReply> Yanitlar { get; set; } = new List<TicketReply>();

        public ICollection<TicketHistory> Gecmis { get; set; } = new List<TicketHistory>();
    }
}
