using System.ComponentModel.DataAnnotations;

namespace HelpDesk.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olmalıdır.")]
        [Display(Name = "Kategori Adı")]
        public string Ad { get; set; } = string.Empty;

        [Display(Name = "Aktif mi?")]
        public bool AktifMi { get; set; } = true;
    }
}
