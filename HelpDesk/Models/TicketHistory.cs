using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDesk.Models
{
    // Bir talep üzerinde yapılan her işlemin (oluşturma, atama, durum değişikliği,
    // yanıt, kapatma...) kalıcı kaydı. Talep detayında zaman çizelgesi olarak gösterilir
    // ve kimin ne zaman ne yaptığının izlenebilmesini sağlar (denetim/etkinlik geçmişi).
    public class TicketHistory
    {
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        // İşlemi yapan kullanıcı. Kullanıcı silinirse kayıt korunur (AktorId null olur).
        public string? AktorId { get; set; }
        [ForeignKey("AktorId")]
        public ApplicationUser? Aktor { get; set; }

        public TicketIslemTuru Tur { get; set; }

        [StringLength(500)]
        public string Aciklama { get; set; } = string.Empty;

        // Dahili işlem geçmişi kaydı: müşteriye gösterilmez (örn. dahili not eklendi).
        public bool DahiliMi { get; set; } = false;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    }
}
