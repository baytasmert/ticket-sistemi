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

        // In-memory rate limiting: IP -> (attempts, last attempt time)
        private static Dictionary<string, (int attempts, DateTime lastAttempt)> _loginAttempts = new();
        private const int MAX_LOGIN_ATTEMPTS = 5;
        private const int LOCKOUT_DURATION_MINUTES = 15;

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
            // Rate limiting check
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            if (_loginAttempts.TryGetValue(clientIp, out var attempt))
            {
                if (DateTime.UtcNow - attempt.lastAttempt < TimeSpan.FromMinutes(LOCKOUT_DURATION_MINUTES))
                {
                    if (attempt.attempts >= MAX_LOGIN_ATTEMPTS)
                    {
                        ModelState.AddModelError(string.Empty, $"Çok fazla başarısız deneme. {LOCKOUT_DURATION_MINUTES} dakika sonra tekrar deneyin.");
                        return View(model);
                    }
                }
                else
                {
                    // Reset attempts after lockout period
                    _loginAttempts.Remove(clientIp);
                }
            }

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
                    // Reset login attempts on success
                    _loginAttempts.Remove(clientIp);
                    return RedirectToAction("Index", "Home");
                }

                // Track failed attempt
                if (_loginAttempts.TryGetValue(clientIp, out var failedAttempt))
                {
                    _loginAttempts[clientIp] = (failedAttempt.attempts + 1, DateTime.UtcNow);
                }
                else
                {
                    _loginAttempts[clientIp] = (1, DateTime.UtcNow);
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
    }
}
