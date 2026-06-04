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
    [Authorize(Roles = "SupportAgent,Admin")]
    public class SupportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SupportController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET /Support/Dashboard — istatistik kartları + bekleyen talepler + kategori dağılımı
        public async Task<IActionResult> Dashboard()
        {
            var agentId = _userManager.GetUserId(User)!;

            var tickets = await _context.Tickets
                .Include(t => t.Category)
                .ToListAsync();

            var vm = new SupportDashboardViewModel
            {
                ToplamTalep = tickets.Count,
                AcikTalep = tickets.Count(t => t.Durum == TicketDurumu.Açık),
                IslemdeTalep = tickets.Count(t => t.Durum == TicketDurumu.İşlemde),
                CozulduTalep = tickets.Count(t => t.Durum == TicketDurumu.Çözüldü),
                KapatildiTalep = tickets.Count(t => t.Durum == TicketDurumu.Kapatıldı),
                BanaAtanan = tickets.Count(t => t.AtananAjanId == agentId
                                             && t.Durum != TicketDurumu.Kapatıldı),
                KategoriDagilimi = tickets
                    .GroupBy(t => t.Category?.Ad ?? "Kategorisiz")
                    .Select(g => new KategoriSayisi { Kategori = g.Key, Sayi = g.Count() })
                    .OrderByDescending(x => x.Sayi)
                    .ToList(),
                SonTalepler = tickets
                    .Where(t => t.Durum == TicketDurumu.Açık || t.Durum == TicketDurumu.İşlemde)
                    .OrderByDescending(t => t.OlusturmaTarihi)
                    .Take(5)
                    .ToList()
            };

            return View(vm);
        }

        // GET /Support/Index?durum=&kategoriId=&oncelik=&arama=
        public async Task<IActionResult> Index(string? durum, int? kategoriId, string? oncelik, string? arama)
        {
            var query = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Musteri)
                .Include(t => t.AtananAjan)
                .AsQueryable();

            // LINQ filtreleme — durum, kategori, öncelik, metin araması
            if (!string.IsNullOrWhiteSpace(durum) && Enum.TryParse<TicketDurumu>(durum, out var parsedDurum))
                query = query.Where(t => t.Durum == parsedDurum);

            if (kategoriId.HasValue)
                query = query.Where(t => t.CategoryId == kategoriId.Value);

            if (!string.IsNullOrWhiteSpace(oncelik) && Enum.TryParse<TicketOnceligi>(oncelik, out var parsedOncelik))
                query = query.Where(t => t.Oncelik == parsedOncelik);

            if (!string.IsNullOrWhiteSpace(arama))
            {
                var aramaLower = arama.ToLower();
                query = query.Where(t =>
                    t.Baslik.ToLower().Contains(aramaLower) ||
                    t.Aciklama.ToLower().Contains(aramaLower) ||
                    (t.Musteri != null && t.Musteri.AdSoyad.ToLower().Contains(aramaLower)));
            }

            var tickets = await query
                .OrderByDescending(t => t.OlusturmaTarihi)
                .ToListAsync();

            var kategoriler = await _context.Categories
                .Where(c => c.AktifMi)
                .OrderBy(c => c.Ad)
                .ToListAsync();

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

            var query = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Musteri)
                .Where(t => t.AtananAjanId == agentId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(durum) && Enum.TryParse<TicketDurumu>(durum, out var parsedDurum))
                query = query.Where(t => t.Durum == parsedDurum);

            var tickets = await query
                .OrderByDescending(t => t.OlusturmaTarihi)
                .ToListAsync();

            ViewBag.SeciliDurum = durum;
            return View(tickets);
        }

        // GET /Support/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Musteri)
                .Include(t => t.AtananAjan)
                .Include(t => t.Yanitlar)
                    .ThenInclude(r => r.Yazar)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            ViewBag.CurrentUserId = _userManager.GetUserId(User);

            var model = new TicketDetailsViewModel
            {
                Ticket = ticket,
                Yanitlar = ticket.Yanitlar.OrderBy(r => r.OlusturmaTarihi).ToList(),
                YeniYanit = new TicketReplyViewModel { TicketId = id }
            };

            return View(model);
        }

        // POST /Support/AssignTicket/5 — talebi mevcut ajana ata ve durumu İşlemde yap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            var agentId = _userManager.GetUserId(User)!;
            ticket.AtananAjanId = agentId;
            ticket.GuncellenmeTarihi = DateTime.Now;

            if (ticket.Durum == TicketDurumu.Açık)
                ticket.Durum = TicketDurumu.İşlemde;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Talep başarıyla üstlenildi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST /Support/AddReply — yanıt ekle, isteğe bağlı durum güncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReply(int ticketId, string mesaj, TicketDurumu? yeniDurum)
        {
            if (string.IsNullOrWhiteSpace(mesaj))
            {
                TempData["Error"] = "Yanıt mesajı boş olamaz.";
                return RedirectToAction(nameof(Details), new { id = ticketId });
            }

            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return NotFound();

            var agentId = _userManager.GetUserId(User)!;
            var reply = new TicketReply
            {
                TicketId = ticketId,
                YazarId = agentId,
                Mesaj = mesaj.Trim(),
                OlusturmaTarihi = DateTime.Now
            };

            _context.TicketReplies.Add(reply);
            ticket.GuncellenmeTarihi = DateTime.Now;

            if (yeniDurum.HasValue)
                ticket.Durum = yeniDurum.Value;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Yanıt başarıyla eklendi.";
            return RedirectToAction(nameof(Details), new { id = ticketId });
        }

        // POST /Support/UpdateStatus — durum güncelleme (yanıtsız)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, TicketDurumu durum)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            ticket.Durum = durum;
            ticket.GuncellenmeTarihi = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Durum '{durum}' olarak güncellendi.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
