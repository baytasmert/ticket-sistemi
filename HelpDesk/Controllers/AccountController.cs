using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HelpDesk.Models;
using HelpDesk.ViewModels;

namespace HelpDesk.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    AdSoyad = model.AdSoyad,
                    Telefon = model.Telefon,
                    AktifMi = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Customer");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("SupportAgent") || roles.Contains("Admin"))
                    {
                        ModelState.AddModelError(string.Empty, "Destek ekibi üyesi misiniz? Lütfen /staff/login adresini kullanın.");
                        return View(model);
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (user != null && !user.AktifMi)
                    {
                        await _signInManager.SignOutAsync();
                        ModelState.AddModelError(string.Empty, "Hesabınız devre dışı bırakılmıştır. Yönetici ile iletişime geçin.");
                        return View(model);
                    }
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Email veya şifre hatalı.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var roles = await _userManager.GetRolesAsync(user);
            var model = new ProfileViewModel
            {
                AdSoyad = user.AdSoyad,
                Telefon = user.Telefon,
                Email = user.Email ?? "",
                Rol = roles.FirstOrDefault() ?? "Bilinmiyor"
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            user.AdSoyad = model.AdSoyad;
            user.Telefon = model.Telefon;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Profil başarıyla güncellendi.";
            return RedirectToAction("Profile");
        }
    }
}
