using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HelpDesk.Models;
using HelpDesk.Services;
using HelpDesk.ViewModels;

namespace HelpDesk.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly LoginThrottle _loginThrottle;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            LoginThrottle loginThrottle)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _loginThrottle = loginThrottle;
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
            // Hız sınırlaması: kilitliyse kalan süreyi göster.
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var kalan = _loginThrottle.GetLockoutRemaining(clientIp);
            if (kalan.HasValue)
            {
                ModelState.AddModelError(string.Empty,
                    $"Çok fazla başarısız deneme. {Math.Ceiling(kalan.Value.TotalMinutes)} dakika sonra tekrar deneyin.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("SupportAgent") || roles.Contains("Admin"))
                    {
                        // Personel hesapları müşteri portalından giriş yapamaz. Hesabın
                        // personele ait olduğunu ifşa etmemek için (kullanıcı sayımı /
                        // enumeration önlemi) hatalı parolayla aynı genel mesaj verilir.
                        _loginThrottle.RegisterFailure(clientIp);
                        ModelState.AddModelError(string.Empty, "Email veya şifre hatalı.");
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
                    _loginThrottle.Reset(clientIp);
                    return RedirectToAction("Index", "Home");
                }

                _loginThrottle.RegisterFailure(clientIp);
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
    }
}
