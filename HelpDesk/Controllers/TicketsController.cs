using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Data;
using HelpDesk.Models;
using HelpDesk.ViewModels;

namespace HelpDesk.Controllers
{
    [Authorize(Roles = "Customer")]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET /Tickets
        public async Task<IActionResult> Index(string? durum)
        {
            var userId = _userManager.GetUserId(User)!;
            var query = _context.Tickets
                .Include(t => t.Category)
                .Where(t => t.MusteriId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(durum) && Enum.TryParse<TicketDurumu>(durum, out var parsedDurum))
                query = query.Where(t => t.Durum == parsedDurum);

            var tickets = await query
                .OrderByDescending(t => t.OlusturmaTarihi)
                .ToListAsync();

            ViewBag.SeciliDurum = durum;
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
            var ticket = new Ticket
            {
                Baslik = model.Baslik,
                Aciklama = model.Aciklama,
                CategoryId = model.CategoryId,
                Oncelik = model.Oncelik,
                Durum = TicketDurumu.Açık,
                MusteriId = userId,
                OlusturmaTarihi = DateTime.Now,
                GuncellenmeTarihi = DateTime.Now
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Destek talebiniz başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Tickets/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var ticket = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Musteri)
                .Include(t => t.AtananAjan)
                .Include(t => t.Yanitlar)
                    .ThenInclude(r => r.Yazar)
                .FirstOrDefaultAsync(t => t.Id == id && t.MusteriId == userId);

            if (ticket == null) return NotFound();

            var model = new TicketDetailsViewModel
            {
                Ticket = ticket,
                Yanitlar = ticket.Yanitlar.OrderBy(r => r.OlusturmaTarihi).ToList(),
                YeniYanit = new TicketReplyViewModel { TicketId = id }
            };

            return View(model);
        }

        // POST /Tickets/AddReply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReply(TicketReplyViewModel model)
        {
            var userId = _userManager.GetUserId(User)!;
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == model.TicketId && t.MusteriId == userId);

            if (ticket == null) return NotFound();

            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Details), new { id = model.TicketId });

            var reply = new TicketReply
            {
                TicketId = model.TicketId,
                YazarId = userId,
                Mesaj = model.Mesaj,
                OlusturmaTarihi = DateTime.Now
            };

            ticket.GuncellenmeTarihi = DateTime.Now;
            if (ticket.Durum == TicketDurumu.Çözüldü || ticket.Durum == TicketDurumu.Kapatıldı)
                ticket.Durum = TicketDurumu.Açık;

            _context.TicketReplies.Add(reply);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Yanıtınız eklendi.";
            return RedirectToAction(nameof(Details), new { id = model.TicketId });
        }

        // POST /Tickets/Close — müşteri kendi talebini kapatır
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == id && t.MusteriId == userId);

            if (ticket == null) return NotFound();

            if (ticket.Durum != TicketDurumu.Kapatıldı)
            {
                ticket.Durum = TicketDurumu.Kapatıldı;
                ticket.GuncellenmeTarihi = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Talebiniz kapatıldı.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<List<SelectListItem>> GetKategoriListesiAsync()
        {
            return await _context.Categories
                .Where(c => c.AktifMi)
                .OrderBy(c => c.Ad)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Ad })
                .ToListAsync();
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
