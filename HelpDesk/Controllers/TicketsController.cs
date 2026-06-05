using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using HelpDesk.Models;
using HelpDesk.Services;
using HelpDesk.ViewModels;

namespace HelpDesk.Controllers
{
    [Authorize(Roles = "Customer")]
    public class TicketsController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketsController(ITicketService ticketService, UserManager<ApplicationUser> userManager)
        {
            _ticketService = ticketService;
            _userManager = userManager;
        }

        // GET /Tickets
        public async Task<IActionResult> Index(string? durum)
        {
            var userId = _userManager.GetUserId(User)!;
            var parsedDurum = ParseDurum(durum);

            var tickets = await _ticketService.GetMusteriTalepleriAsync(userId, parsedDurum);
            var sayilar = await _ticketService.GetMusteriDurumSayilariAsync(userId);

            ViewBag.SeciliDurum = durum;
            ViewBag.DurumSayilari = sayilar;
            ViewBag.ToplamSayi = sayilar.Values.Sum();
            return View(tickets);
        }

        // GET /Tickets/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new TicketCreateViewModel
            {
                Kategoriler = await GetKategoriListesiAsync(),
                OncelikSecenekleri = GetOncelikListesi()
            };
            return View(model);
        }

        // POST /Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Kategoriler = await GetKategoriListesiAsync();
                model.OncelikSecenekleri = GetOncelikListesi();
                return View(model);
            }

            var userId = _userManager.GetUserId(User)!;
            await _ticketService.OlusturAsync(userId, model);

            TempData["Success"] = "Destek talebiniz başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Tickets/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var model = await _ticketService.GetMusteriDetayAsync(id, userId);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST /Tickets/AddReply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReply(TicketReplyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Yanıt mesajı boş olamaz.";
                return RedirectToAction(nameof(Details), new { id = model.TicketId });
            }

            var userId = _userManager.GetUserId(User)!;
            var ok = await _ticketService.MusteriYanitEkleAsync(model.TicketId, userId, model.Mesaj);
            if (!ok) return NotFound();

            TempData["Success"] = "Yanıtınız eklendi.";
            return RedirectToAction(nameof(Details), new { id = model.TicketId });
        }

        // POST /Tickets/Close — müşteri kendi talebini kapatır
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var sonuc = await _ticketService.MusteriKapatAsync(id, userId);
            if (sonuc == null) return NotFound();

            if (sonuc == true)
                TempData["Success"] = "Talebiniz kapatıldı.";

            return RedirectToAction(nameof(Details), new { id });
        }

        // Boş/geçersiz değerleri null'a indirgeyen durum ayrıştırıcı.
        private static TicketDurumu? ParseDurum(string? durum) =>
            !string.IsNullOrWhiteSpace(durum) && Enum.TryParse<TicketDurumu>(durum, out var d) ? d : null;

        private async Task<List<SelectListItem>> GetKategoriListesiAsync()
        {
            var kategoriler = await _ticketService.GetAktifKategorilerAsync();
            return kategoriler
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Ad })
                .ToList();
        }

        private static List<SelectListItem> GetOncelikListesi()
        {
            return new List<SelectListItem>
            {
                new() { Value = "0", Text = "Düşük" },
                new() { Value = "1", Text = "Orta", Selected = true },
                new() { Value = "2", Text = "Yüksek" },
                new() { Value = "3", Text = "Kritik" }
            };
        }
    }
}
