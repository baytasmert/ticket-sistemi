using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using HelpDesk.Models;

namespace HelpDesk.ViewModels
{
    public class TicketCreateViewModel
    {
        [Required(ErrorMessage = "Başlık zorunludur.")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olmalıdır.")]
        [Display(Name = "Başlık")]
        public string Baslik { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [StringLength(5000, ErrorMessage = "Açıklama en fazla 5000 karakter olmalıdır.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }

        [Display(Name = "Öncelik")]
        public TicketOnceligi Oncelik { get; set; } = TicketOnceligi.Orta;

        public List<SelectListItem> Kategoriler { get; set; } = new();
        public List<SelectListItem> OncelikSecenekleri { get; set; } = new();
    }
}
