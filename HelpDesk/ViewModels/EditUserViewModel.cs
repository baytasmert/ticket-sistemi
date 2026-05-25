using System.ComponentModel.DataAnnotations;

namespace HelpDesk.ViewModels
{
    public class EditUserViewModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olmalıdır.")]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(15, ErrorMessage = "Telefon en fazla 15 karakter olmalıdır.")]
        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        [Display(Name = "Departman")]
        public string? Departman { get; set; }
    }
}
