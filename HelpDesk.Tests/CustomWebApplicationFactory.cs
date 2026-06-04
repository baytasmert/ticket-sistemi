using HelpDesk.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Tests;

/// <summary>
/// Uygulamayı bellek-içi bir test sunucusunda (TestServer) ayağa kaldırır.
/// Her fabrika örneği, gerçek veritabanına dokunmamak için kendine ait
/// geçici bir SQLite dosyası kullanır. Program.cs'deki migration + seed
/// (admin, support hesapları ve kategoriler) bu test veritabanında da çalışır.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(), $"helpdesk_test_{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Uygulamanın kaydettiği DbContext yapılandırmasını kaldırıp,
            // bu test örneğine özel benzersiz SQLite dosyasına yönlendiriyoruz.
            // Böylece test sınıfları birbirinin ve gerçek veritabanının verisine
            // dokunmaz (paralel çalışmada kilitlenme olmaz).
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite($"Data Source={_dbPath}"));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && File.Exists(_dbPath))
        {
            try { File.Delete(_dbPath); } catch { /* dosya kilitliyse yoksay */ }
        }
    }
}
