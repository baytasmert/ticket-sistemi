using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HelpDesk.Models;
using HelpDesk.Services;
using HelpDesk.ViewModels;

namespace HelpDesk.Controllers
{
    [Route("staff")]
    public class StaffController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LoginThrottle _loginThrottle;

        public StaffController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            LoginThrottle loginThrottle)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _loginThrottle = loginThrottle;
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("login")]
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
                    if (!roles.Contains("SupportAgent") && !roles.Contains("Admin"))
                    {
                        ModelState.AddModelError(string.Empty, "Bu hesap destek ekibi için değildir.");
                        return View(model);
                    }

                    if (!user.AktifMi)
                    {
                        ModelState.AddModelError(string.Empty, "Hesabınız devre dışı bırakılmıştır.");
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
                    _loginThrottle.Reset(clientIp);
                    return RedirectToAction("Dashboard", "Support");
                }

                _loginThrottle.RegisterFailure(clientIp);
                ModelState.AddModelError(string.Empty, "Email veya şifre hatalı.");
            }

            return View(model);
        }
    }
}
