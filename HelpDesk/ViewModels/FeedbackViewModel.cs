using System.ComponentModel.DataAnnotations;

namespace HelpDesk.ViewModels
{
    public class FeedbackViewModel
    {
        [Required(ErrorMessage = "Kategori seçiniz.")]
        [Display(Name = "Kategori")]
        public string Kategori { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mesaj zorunludur.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Mesaj en az 10, en fazla 2000 karakter olmalıdır.")]
        [Display(Name = "Mesajınız")]
        public string Mesaj { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Geçerli bir email girin.")]
        [Display(Name = "Email (İsteğe bağlı)")]
        public string? Email { get; set; }
    }
}
