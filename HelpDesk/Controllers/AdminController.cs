using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HelpDesk.Models;
using HelpDesk.ViewModels;

namespace HelpDesk.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
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

            var stats = new Dictionary<string, int>
            {
                { "ToplamKullanici", users.Count },
                { "AdminSayisi", userViewModels.Count(u => u.Rol == "Admin") },
                { "SupportAgentSayisi", userViewModels.Count(u => u.Rol == "SupportAgent") },
                { "CustomerSayisi", userViewModels.Count(u => u.Rol == "Customer") }
            };

            ViewBag.Stats = stats;

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
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            await _userManager.AddToRoleAsync(user, newRole);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.AktifMi = !user.AktifMi;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index");
        }
    }
}
