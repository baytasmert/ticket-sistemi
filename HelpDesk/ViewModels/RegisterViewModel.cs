using System.ComponentModel.DataAnnotations;

namespace HelpDesk.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olmalıdır.")]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi girin.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(15, ErrorMessage = "Telefon numarası en fazla 15 karakter olmalıdır.")]
        [Display(Name = "Telefon (Opsiyonel)")]
        public string? Telefon { get; set; }

        // Şifre KARMAŞIKLIK kuralları (büyük/küçük/rakam/özel karakter) tek yerde:
        // Program.cs'teki Identity politikası. Burada yalnızca anlık istemci geri
        // bildirimi için zorunluluk + minimum uzunluk bırakıyoruz.
        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Şifreyi Onayla")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
