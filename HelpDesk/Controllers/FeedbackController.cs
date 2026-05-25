using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HelpDesk.Data;
using HelpDesk.Models;
using HelpDesk.ViewModels;

namespace HelpDesk.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Kategoriler = new[] { "Geri Bildirim", "İyileştirme Önerisi", "Şikayet" };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FeedbackViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Kategoriler = new[] { "Geri Bildirim", "İyileştirme Önerisi", "Şikayet" };
                return View(model);
            }

            var feedback = new Feedback
            {
                Kategori = model.Kategori,
                Mesaj = model.Mesaj,
                Email = model.Email,
                CreatedAt = DateTime.Now,
                Okundu = false
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Geri bildiriminiz başarıyla gönderildi. Teşekkürler!";
            return RedirectToAction("Index", "Home");
        }
    }
}
