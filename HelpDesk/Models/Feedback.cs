using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Kategori { get; set; } = string.Empty; // Geri Bildirim, İyileştirme, Şikayet

        [Required]
        [StringLength(2000)]
        public string Mesaj { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool Okundu { get; set; } = false;
    }
}
