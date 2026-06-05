using HelpDesk.Data;
using HelpDesk.Models;
using HelpDesk.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Services
{
    /// <inheritdoc cref="ITicketService"/>
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ────────────────────────── Müşteri akışı ──────────────────────────

        public async Task<List<Ticket>> GetMusteriTalepleriAsync(string musteriId, TicketDurumu? durum)
        {
            var query = _context.Tickets
                .Include(t => t.Category)
                .Where(t => t.MusteriId == musteriId);

            if (durum.HasValue)
                query = query.Where(t => t.Durum == durum.Value);

            return await query.OrderByDescending(t => t.OlusturmaTarihi).ToListAsync();
        }

        public async Task<Dictionary<TicketDurumu, int>> GetMusteriDurumSayilariAsync(string musteriId)
        {
            // Tüm durumların sayısı (filtreden bağımsız) — özet kartları için.
            return await _context.Tickets
                .Where(t => t.MusteriId == musteriId)
                .GroupBy(t => t.Durum)
                .Select(g => new { g.Key, Sayi = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Sayi);
        }

        public async Task<TicketDetailsViewModel?> GetMusteriDetayAsync(int ticketId, string musteriId)
        {
            var ticket = await DetayQuery()
                .FirstOrDefaultAsync(t => t.Id == ticketId && t.MusteriId == musteriId);
            if (ticket == null) return null;

            // Müşteri dahili (yalnızca ekip) kayıtlarını GÖRMEZ.
            return new TicketDetailsViewModel
            {
                Ticket = ticket,
                Yanitlar = ticket.Yanitlar.Where(r => !r.DahiliMi)
                                  .OrderBy(r => r.OlusturmaTarihi).ToList(),
                Gecmis = ticket.Gecmis.Where(h => !h.DahiliMi)
                                .OrderByDescending(h => h.OlusturmaTarihi).ToList(),
                YeniYanit = new TicketReplyViewModel { TicketId = ticketId }
            };
        }

        public async Task<Ticket> OlusturAsync(string musteriId, TicketCreateViewModel model)
        {
            var simdi = DateTime.Now;
            var ticket = new Ticket
            {
                Baslik = model.Baslik.Trim(),
                Aciklama = model.Aciklama.Trim(),
                CategoryId = model.CategoryId,
                Oncelik = model.Oncelik,
                Durum = TicketDurumu.Açık,
                MusteriId = musteriId,
                OlusturmaTarihi = simdi,
                GuncellenmeTarihi = simdi
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync(); // Id geçmiş kaydı için gerekli

            Log(ticket.Id, musteriId, TicketIslemTuru.Olusturuldu,
                $"Talep oluşturuldu (Öncelik: {ticket.Oncelik}).");
            await _context.SaveChangesAsync();

            return ticket;
        }

        public async Task<bool> MusteriYanitEkleAsync(int ticketId, string musteriId, string mesaj)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == ticketId && t.MusteriId == musteriId);
            if (ticket == null) return false;

            var simdi = DateTime.Now;
            _context.TicketReplies.Add(new TicketReply
            {
                TicketId = ticketId,
                YazarId = musteriId,
                Mesaj = mesaj.Trim(),
                OlusturmaTarihi = simdi
            });
            ticket.GuncellenmeTarihi = simdi;
            Log(ticketId, musteriId, TicketIslemTuru.YanitEklendi, "Müşteri yanıt ekledi.");

            // Çözülmüş/kapatılmış bir talebe yanıt gelirse yeniden açılır.
            if (ticket.Durum == TicketDurumu.Çözüldü || ticket.Durum == TicketDurumu.Kapatıldı)
            {
                ticket.Durum = TicketDurumu.Açık;
                Log(ticketId, musteriId, TicketIslemTuru.YenidenAcildi,
                    "Müşteri yanıtıyla talep yeniden açıldı.");
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool?> MusteriKapatAsync(int ticketId, string musteriId)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == ticketId && t.MusteriId == musteriId);
            if (ticket == null) return null;
            if (ticket.Durum == TicketDurumu.Kapatıldı) return false;

            ticket.Durum = TicketDurumu.Kapatıldı;
            ticket.GuncellenmeTarihi = DateTime.Now;
            Log(ticketId, musteriId, TicketIslemTuru.Kapatildi, "Müşteri talebi kapattı.");
            await _context.SaveChangesAsync();
            return true;
        }

        // ────────────────────────── Destek / Admin ──────────────────────────

        public async Task<SupportDashboardViewModel> GetDashboardAsync(string agentId)
        {
            // Sayımlar veritabanı tarafında yapılır (tüm talepleri belleğe çekmeden).
            var durumSayilari = await _context.Tickets
                .GroupBy(t => t.Durum)
                .Select(g => new { Durum = g.Key, Sayi = g.Count() })
                .ToDictionaryAsync(x => x.Durum, x => x.Sayi);

            int Say(TicketDurumu d) => durumSayilari.TryGetValue(d, out var n) ? n : 0;

            var katSayilari = await _context.Tickets
                .GroupBy(t => t.CategoryId)
                .Select(g => new { CategoryId = g.Key, Sayi = g.Count() })
                .ToListAsync();
            var katAdlari = await _context.Categories.ToDictionaryAsync(c => c.Id, c => c.Ad);

            return new SupportDashboardViewModel
            {
                ToplamTalep = durumSayilari.Values.Sum(),
                AcikTalep = Say(TicketDurumu.Açık),
                IslemdeTalep = Say(TicketDurumu.İşlemde),
                CozulduTalep = Say(TicketDurumu.Çözüldü),
                KapatildiTalep = Say(TicketDurumu.Kapatıldı),
                BanaAtanan = await _context.Tickets.CountAsync(
                    t => t.AtananAjanId == agentId && t.Durum != TicketDurumu.Kapatıldı),
                AtanmamisTalep = await _context.Tickets.CountAsync(
                    t => t.AtananAjanId == null && t.Durum != TicketDurumu.Kapatıldı),
                KategoriDagilimi = katSayilari
                    .Select(x => new KategoriSayisi
                    {
                        Kategori = katAdlari.TryGetValue(x.CategoryId, out var ad) ? ad : "Kategorisiz",
                        Sayi = x.Sayi
                    })
                    .OrderByDescending(x => x.Sayi)
                    .ToList(),
                SonTalepler = await _context.Tickets
                    .Include(t => t.Category)
                    .Include(t => t.Musteri)
                    .Where(t => t.Durum == TicketDurumu.Açık || t.Durum == TicketDurumu.İşlemde)
                    .OrderByDescending(t => t.OlusturmaTarihi)
                    .Take(5)
                    .ToListAsync()
            };
        }

        public async Task<List<Ticket>> FiltreleAsync(string? durum, int? kategoriId, string? oncelik, string? arama)
        {
            var query = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Musteri)
                .Include(t => t.AtananAjan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(durum) && Enum.TryParse<TicketDurumu>(durum, out var d))
                query = query.Where(t => t.Durum == d);

            if (kategoriId.HasValue)
                query = query.Where(t => t.CategoryId == kategoriId.Value);

            if (!string.IsNullOrWhiteSpace(oncelik) && Enum.TryParse<TicketOnceligi>(oncelik, out var o))
                query = query.Where(t => t.Oncelik == o);

            if (!string.IsNullOrWhiteSpace(arama))
            {
                var q = arama.ToLower();
                query = query.Where(t =>
                    t.Baslik.ToLower().Contains(q) ||
                    t.Aciklama.ToLower().Contains(q) ||
                    (t.Musteri != null && t.Musteri.AdSoyad.ToLower().Contains(q)));
            }

            return await query.OrderByDescending(t => t.OlusturmaTarihi).ToListAsync();
        }

        public async Task<List<Ticket>> GetAtananlarAsync(string agentId, TicketDurumu? durum)
        {
            var query = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Musteri)
                .Where(t => t.AtananAjanId == agentId);

            if (durum.HasValue)
                query = query.Where(t => t.Durum == durum.Value);

            return await query.OrderByDescending(t => t.OlusturmaTarihi).ToListAsync();
        }

        public async Task<TicketDetailsViewModel?> GetDestekDetayAsync(int ticketId)
        {
            var ticket = await DetayQuery().FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null) return null;

            // Destek/Admin TÜM kayıtları (dahili dahil) görür.
            return new TicketDetailsViewModel
            {
                Ticket = ticket,
                Yanitlar = ticket.Yanitlar.OrderBy(r => r.OlusturmaTarihi).ToList(),
                Gecmis = ticket.Gecmis.OrderByDescending(h => h.OlusturmaTarihi).ToList(),
                YeniYanit = new TicketReplyViewModel { TicketId = ticketId }
            };
        }

        public async Task<bool> AtaAsync(int ticketId, string actorId, string? hedefAjanId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return false;

            var atananId = string.IsNullOrWhiteSpace(hedefAjanId) ? actorId : hedefAjanId;
            ticket.AtananAjanId = atananId;
            ticket.GuncellenmeTarihi = DateTime.Now;

            // Atama ile birlikte açık talep otomatik işleme alınır.
            if (ticket.Durum == TicketDurumu.Açık)
                ticket.Durum = TicketDurumu.İşlemde;

            if (atananId == actorId)
            {
                Log(ticketId, actorId, TicketIslemTuru.Atandi, "Talep üstlenildi.");
            }
            else
            {
                var ajanAdi = (await _userManager.FindByIdAsync(atananId))?.AdSoyad ?? "bir temsilci";
                Log(ticketId, actorId, TicketIslemTuru.Atandi, $"Talep {ajanAdi} kişisine atandı.");
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AjanYanitEkleAsync(int ticketId, string ajanId, string mesaj, bool dahili, TicketDurumu? yeniDurum)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return false;

            var simdi = DateTime.Now;
            _context.TicketReplies.Add(new TicketReply
            {
                TicketId = ticketId,
                YazarId = ajanId,
                Mesaj = mesaj.Trim(),
                DahiliMi = dahili,
                OlusturmaTarihi = simdi
            });
            ticket.GuncellenmeTarihi = simdi;

            Log(ticketId, ajanId,
                dahili ? TicketIslemTuru.DahiliNot : TicketIslemTuru.YanitEklendi,
                dahili ? "Dahili not eklendi." : "Destek yanıt ekledi.",
                dahili);

            if (yeniDurum.HasValue && yeniDurum.Value != ticket.Durum)
            {
                var eski = ticket.Durum;
                ticket.Durum = yeniDurum.Value;
                Log(ticketId, ajanId,
                    yeniDurum.Value == TicketDurumu.Kapatıldı ? TicketIslemTuru.Kapatildi : TicketIslemTuru.DurumDegisti,
                    $"Durum '{eski}' → '{yeniDurum.Value}' olarak güncellendi.");
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DurumGuncelleAsync(int ticketId, string actorId, TicketDurumu durum)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return false;

            if (ticket.Durum != durum)
            {
                var eski = ticket.Durum;
                ticket.Durum = durum;
                ticket.GuncellenmeTarihi = DateTime.Now;
                Log(ticketId, actorId,
                    durum == TicketDurumu.Kapatıldı ? TicketIslemTuru.Kapatildi : TicketIslemTuru.DurumDegisti,
                    $"Durum '{eski}' → '{durum}' olarak güncellendi.");
                await _context.SaveChangesAsync();
            }
            return true;
        }

        // ────────────────────────── Ortak ──────────────────────────

        public async Task<List<Category>> GetAktifKategorilerAsync() =>
            await _context.Categories.Where(c => c.AktifMi).OrderBy(c => c.Ad).ToListAsync();

        public async Task<List<ApplicationUser>> GetDestekAjanlariAsync()
        {
            var ajanlar = await _userManager.GetUsersInRoleAsync("SupportAgent");
            return ajanlar.Where(u => u.AktifMi).OrderBy(u => u.AdSoyad).ToList();
        }

        // ────────────────────────── Yardımcılar ──────────────────────────

        // Talep detayını ilişkileriyle birlikte yükleyen ortak sorgu.
        private IQueryable<Ticket> DetayQuery() =>
            _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Musteri)
                .Include(t => t.AtananAjan)
                .Include(t => t.Yanitlar).ThenInclude(r => r.Yazar)
                .Include(t => t.Gecmis).ThenInclude(h => h.Aktor);

        // İşlem geçmişine kayıt ekler (kaydı çağıran SaveChangesAsync ile persist eder).
        private void Log(int ticketId, string? actorId, TicketIslemTuru tur, string aciklama, bool dahili = false)
        {
            _context.TicketHistories.Add(new TicketHistory
            {
                TicketId = ticketId,
                AktorId = actorId,
                Tur = tur,
                Aciklama = aciklama,
                DahiliMi = dahili,
                OlusturmaTarihi = DateTime.Now
            });
        }
    }
}
