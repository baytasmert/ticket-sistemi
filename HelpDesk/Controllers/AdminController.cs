using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            var feedbacks = _context.Feedbacks.ToList();

            var stats = new Dictionary<string, int>
            {
                { "ToplamKullanici", allUsers.Count },
                { "ToplamMusteri", 0 },
                { "ToplamDestek", 0 },
                { "ToplamAdmin", 0 },
                { "OkunmamisFeedback", feedbacks.Count(f => !f.Okundu) },
                { "ToplamFeedback", feedbacks.Count }
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
            await _userManager.DeleteAsync(user);

            TempData["Success"] = $"{email} kullanıcısı silindi.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Feedbacks()
        {
            var feedbacks = _context.Feedbacks.OrderByDescending(f => f.CreatedAt).ToList();
            return View(feedbacks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int feedbackId)
        {
            var feedback = await _context.Feedbacks.FindAsync(feedbackId);
            if (feedback == null) return NotFound();

            feedback.Okundu = !feedback.Okundu;
            _context.Feedbacks.Update(feedback);
            await _context.SaveChangesAsync();

            return RedirectToAction("Feedbacks");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFeedback(int feedbackId)
        {
            var feedback = await _context.Feedbacks.FindAsync(feedbackId);
            if (feedback == null) return NotFound();

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Geri bildirim silindi.";
            return RedirectToAction("Feedbacks");
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
    }
}
