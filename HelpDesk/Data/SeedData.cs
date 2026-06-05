using Microsoft.AspNetCore.Identity;
using HelpDesk.Models;

namespace HelpDesk.Data
{
    public static class SeedData
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var roles = new[] { "Admin", "Customer", "SupportAgent" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "admin@helpdesk.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    AdSoyad = "Administrator",
                    Telefon = "",
                    AktifMi = true,
                    EmailConfirmed = true
                };

                // Şifre koda gömülü DEĞİL: Azure'da App Setting (ADMIN_PASSWORD)
                // olarak verilir; lokalde tanımlı değilse geliştirme varsayılanı kullanılır.
                var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin123!";
                var result = await userManager.CreateAsync(newAdmin, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            var supportEmail = "support@helpdesk.com";
            var supportUser = await userManager.FindByEmailAsync(supportEmail);

            if (supportUser == null)
            {
                var newSupport = new ApplicationUser
                {
                    UserName = supportEmail,
                    Email = supportEmail,
                    AdSoyad = "Destek Personeli",
                    Telefon = "",
                    Departman = "Teknik Destek",
                    AktifMi = true,
                    EmailConfirmed = true
                };

                var supportPassword = Environment.GetEnvironmentVariable("SUPPORT_PASSWORD") ?? "Support123!";
                var result = await userManager.CreateAsync(newSupport, supportPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newSupport, "SupportAgent");
                }
            }
        }

        public static async Task SeedCategoriesAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (context.Categories.Any()) return;

            var defaultCategories = new[]
            {
                new Category { Ad = "Teknik Sorun", AktifMi = true },
                new Category { Ad = "Fatura & Ödeme", AktifMi = true },
                new Category { Ad = "Hesap & Erişim", AktifMi = true },
                new Category { Ad = "Genel Bilgi Talebi", AktifMi = true },
                new Category { Ad = "Geri Bildirim / Öneri", AktifMi = true },
                new Category { Ad = "Diğer", AktifMi = true }
            };

            context.Categories.AddRange(defaultCategories);
            await context.SaveChangesAsync();
        }
    }
}
