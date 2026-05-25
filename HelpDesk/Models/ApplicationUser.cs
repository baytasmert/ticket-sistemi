using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olmalıdır.")]
        public string AdSoyad { get; set; } = string.Empty;

        [StringLength(15, ErrorMessage = "Telefon numarası en fazla 15 karakter olmalıdır.")]
        public string? Telefon { get; set; }

        public bool AktifMi { get; set; } = true;
    }
}
