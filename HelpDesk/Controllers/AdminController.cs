using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models;
using HelpDesk.ViewModels;
using HelpDesk.Data;

namespace HelpDesk.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var allUsers = _userManager.Users.ToList();
            var tickets = _context.Tickets.ToList();
            var simdi = DateTime.Now;

            var stats = new Dictionary<string, int>
            {
                { "ToplamKullanici", allUsers.Count },
                { "ToplamMusteri", 0 },
                { "ToplamDestek", 0 },
                { "ToplamAdmin", 0 },
                // Bu ay açılan talepler
                { "BuAyAcilan", tickets.Count(t =>
                    t.OlusturmaTarihi.Year == simdi.Year && t.OlusturmaTarihi.Month == simdi.Month) },
                // Bu ay tamamlanan (çözüldü/kapatıldı) talepler
                { "BuAyTamamlanan", tickets.Count(t =>
                    (t.Durum == TicketDurumu.Çözüldü || t.Durum == TicketDurumu.Kapatıldı) &&
                    t.GuncellenmeTarihi.Year == simdi.Year && t.GuncellenmeTarihi.Month == simdi.Month) }
            };

            foreach (var u in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var role = roles.FirstOrDefault() ?? "";
                if (role == "Admin") stats["ToplamAdmin"]++;
                else if (role == "SupportAgent") stats["ToplamDestek"]++;
                else if (role == "Customer") stats["ToplamMusteri"]++;
            }

            ViewBag.Stats = stats;
            return View();
        }

        public async Task<IActionResult> Index(string? search)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                usersQuery = usersQuery.Where(u =>
                    u.Email!.ToLower().Contains(searchLower) ||
                    u.AdSoyad.ToLower().Contains(searchLower));
            }

            var users = usersQuery.ToList();
            var userViewModels = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserListViewModel
                {
                    Id = user.Id,
                    AdSoyad = user.AdSoyad,
                    Email = user.Email ?? "",
                    Rol = roles.FirstOrDefault() ?? "Rol Yok",
                    AktifMi = user.AktifMi
                });
            }

            var allUsers = _userManager.Users.ToList();
            var stats = new Dictionary<string, int>
            {
                { "ToplamKullanici", allUsers.Count },
                { "AdminSayisi", 0 },
                { "SupportAgentSayisi", 0 },
                { "CustomerSayisi", 0 }
            };
            foreach (var u in allUsers)
            {
                var r = await _userManager.GetRolesAsync(u);
                var role = r.FirstOrDefault() ?? "";
                if (role == "Admin") stats["AdminSayisi"]++;
                else if (role == "SupportAgent") stats["SupportAgentSayisi"]++;
                else if (role == "Customer") stats["CustomerSayisi"]++;
            }

            ViewBag.Stats = stats;
            ViewBag.Search = search;

            return View(userViewModels);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault() ?? "Customer";

            ViewBag.UserId = userId;
            ViewBag.UserEmail = user.Email;
            ViewBag.CurrentRole = currentRole;
            ViewBag.AvailableRoles = new[] { "Admin", "Customer", "SupportAgent" };

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            await _userManager.AddToRoleAsync(user, newRole);

            TempData["Success"] = $"{user.Email} kullanıcısının rolü '{newRole}' olarak güncellendi.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.AktifMi = !user.AktifMi;
            await _userManager.UpdateAsync(user);

            var durum = user.AktifMi ? "aktif" : "pasif";
            TempData["Success"] = $"{user.Email} kullanıcısı {durum} yapıldı.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == userId)
            {
                TempData["Error"] = "Kendinizi silemezsiniz.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var email = user.Email;

            if (_context.Tickets.Any(t => t.MusteriId == userId || t.AtananAjanId == userId))
            {
                TempData["Error"] = $"{email} kullanıcısı silinemiyor — kullanıcıya bağlı talepler mevcut.";
                return RedirectToAction("Index");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["Error"] = $"{email} kullanıcısı silinemedi.";
                return RedirectToAction("Index");
            }

            TempData["Success"] = $"{email} kullanıcısı silindi.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            ViewBag.Roller = new[] { "Customer", "SupportAgent", "Admin" };
            ViewBag.Departmanlar = new[] { "Teknik Destek", "Müşteri Hizmetleri", "Fatura & Ödeme", "Genel" };
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault() ?? "Customer";

            var model = new EditUserViewModel
            {
                Id = user.Id,
                AdSoyad = user.AdSoyad,
                Email = user.Email ?? "",
                Telefon = user.Telefon,
                Departman = user.Departman
            };

            ViewBag.UserRole = userRole;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.AdSoyad = model.AdSoyad;
            user.Telefon = model.Telefon;
            user.Departman = model.Departman;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = $"{user.Email} kullanıcısı güncellendi.";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roller = new[] { "Customer", "SupportAgent", "Admin" };
                ViewBag.Departmanlar = new[] { "Teknik Destek", "Müşteri Hizmetleri", "Fatura & Ödeme", "Genel" };
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                AdSoyad = model.AdSoyad,
                Telefon = model.Telefon,
                Departman = model.Departman,
                AktifMi = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                ViewBag.Roller = new[] { "Customer", "SupportAgent", "Admin" };
                ViewBag.Departmanlar = new[] { "Teknik Destek", "Müşteri Hizmetleri", "Fatura & Ödeme", "Genel" };
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, model.Rol);
            TempData["Success"] = $"{model.Email} kullanıcısı '{model.Rol}' rolüyle oluşturuldu.";
            return RedirectToAction("Index");
        }

        // GET /Admin/Categories
        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .Include(c => c.Tickets)
                .OrderBy(c => c.Ad)
                .ToListAsync();
            return View(categories);
        }

        // GET /Admin/CreateCategory
        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View(new CategoryViewModel());
        }

        // POST /Admin/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategoryViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var category = new Category
            {
                Ad = model.Ad,
                AktifMi = model.AktifMi
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{category.Ad}' kategorisi oluşturuldu.";
            return RedirectToAction(nameof(Categories));
        }

        // POST /Admin/ToggleCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCategory(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null) return NotFound();

            category.AktifMi = !category.AktifMi;
            await _context.SaveChangesAsync();

            var durum = category.AktifMi ? "aktif" : "pasif";
            TempData["Success"] = $"'{category.Ad}' kategorisi {durum} yapıldı.";
            return RedirectToAction(nameof(Categories));
        }

        // POST /Admin/DeleteCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var category = await _context.Categories
                .Include(c => c.Tickets)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null) return NotFound();

            if (category.Tickets.Any())
            {
                TempData["Error"] = $"'{category.Ad}' kategorisi silinemiyor — bağlı talepler mevcut.";
                return RedirectToAction(nameof(Categories));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{category.Ad}' kategorisi silindi.";
            return RedirectToAction(nameof(Categories));
        }

        // GET /Admin/AllTickets
        [HttpGet]
        public async Task<IActionResult> AllTickets(string? durum)
        {
            var query = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Musteri)
                .Include(t => t.AtananAjan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(durum) && Enum.TryParse<TicketDurumu>(durum, out var parsedDurum))
                query = query.Where(t => t.Durum == parsedDurum);

            var tickets = await query
                .OrderByDescending(t => t.OlusturmaTarihi)
                .ToListAsync();

            ViewBag.SeciliDurum = durum;
            return View(tickets);
        }
    }
}
