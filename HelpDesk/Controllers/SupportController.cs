using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using HelpDesk.Models;
using HelpDesk.Services;

namespace HelpDesk.Controllers
{
    [Authorize(Roles = "SupportAgent,Admin")]
    public class SupportController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SupportController(ITicketService ticketService, UserManager<ApplicationUser> userManager)
        {
            _ticketService = ticketService;
            _userManager = userManager;
        }

        // GET /Support/Dashboard — istatistik kartları + bekleyen talepler + kategori dağılımı
        public async Task<IActionResult> Dashboard()
        {
            var agentId = _userManager.GetUserId(User)!;
            var vm = await _ticketService.GetDashboardAsync(agentId);
            return View(vm);
        }

        // GET /Support/Index?durum=&kategoriId=&oncelik=&arama=
        public async Task<IActionResult> Index(string? durum, int? kategoriId, string? oncelik, string? arama)
        {
            var tickets = await _ticketService.FiltreleAsync(durum, kategoriId, oncelik, arama);

            var kategoriler = await _ticketService.GetAktifKategorilerAsync();

            ViewBag.SeciliDurum = durum;
            ViewBag.SeciliKategoriId = kategoriId;
            ViewBag.SeciliOncelik = oncelik;
            ViewBag.Arama = arama;
            ViewBag.Kategoriler = new SelectList(kategoriler, "Id", "Ad", kategoriId);

            return View(tickets);
        }

        // GET /Support/AssignedToMe?durum=
        public async Task<IActionResult> AssignedToMe(string? durum)
        {
            var agentId = _userManager.GetUserId(User)!;
            var parsedDurum = ParseDurum(durum);

            var tickets = await _ticketService.GetAtananlarAsync(agentId, parsedDurum);

            ViewBag.SeciliDurum = durum;
            return View(tickets);
        }

        // GET /Support/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var model = await _ticketService.GetDestekDetayAsync(id);
            if (model == null) return NotFound();

            ViewBag.CurrentUserId = _userManager.GetUserId(User);
            // Atama dropdown'u için aktif destek temsilcileri.
            ViewBag.Ajanlar = await _ticketService.GetDestekAjanlariAsync();

            return View(model);
        }

        // POST /Support/AssignTicket/5 — talebi kendine ya da seçilen ajana ata
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTicket(int id, string? ajanId)
        {
            var actorId = _userManager.GetUserId(User)!;
            var ok = await _ticketService.AtaAsync(id, actorId, ajanId);
            if (!ok) return NotFound();

            TempData["Success"] = string.IsNullOrWhiteSpace(ajanId)
                ? "Talep başarıyla üstlenildi."
                : "Talep seçilen temsilciye atandı.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST /Support/AddReply — yanıt/dahili not ekle, isteğe bağlı durum güncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReply(int ticketId, string mesaj, bool dahili, TicketDurumu? yeniDurum)
        {
            if (string.IsNullOrWhiteSpace(mesaj))
            {
                TempData["Error"] = "Yanıt mesajı boş olamaz.";
                return RedirectToAction(nameof(Details), new { id = ticketId });
            }

            if (mesaj.Length > 5000)
            {
                TempData["Error"] = "Mesaj en fazla 5000 karakter olabilir.";
                return RedirectToAction(nameof(Details), new { id = ticketId });
            }

            var agentId = _userManager.GetUserId(User)!;
            var ok = await _ticketService.AjanYanitEkleAsync(ticketId, agentId, mesaj, dahili, yeniDurum);
            if (!ok) return NotFound();

            TempData["Success"] = dahili ? "Dahili not eklendi." : "Yanıt başarıyla eklendi.";
            return RedirectToAction(nameof(Details), new { id = ticketId });
        }

        // POST /Support/UpdateStatus — durum güncelleme (yanıtsız)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, TicketDurumu durum)
        {
            var actorId = _userManager.GetUserId(User)!;
            var ok = await _ticketService.DurumGuncelleAsync(id, actorId, durum);
            if (!ok) return NotFound();

            TempData["Success"] = $"Durum '{durum}' olarak güncellendi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private static TicketDurumu? ParseDurum(string? durum) =>
            !string.IsNullOrWhiteSpace(durum) && Enum.TryParse<TicketDurumu>(durum, out var d) ? d : null;
    }
}
