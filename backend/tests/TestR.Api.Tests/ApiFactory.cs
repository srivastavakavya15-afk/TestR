using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using TestR.Infrastructure.Persistence;

namespace TestR.Api.Tests;

public abstract class ApiFactoryBase : WebApplicationFactory<Program>
{

    private readonly SqliteConnection _keepAlive = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);

        _keepAlive.Open();

        builder.UseSetting("ConnectionStrings:Default", "Data Source=:memory:");
        builder.UseSetting("Auth:Enabled", "false");

        builder.ConfigureServices(services =>
        {

            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_keepAlive));
        });
    }

    public HttpClient CreateApiClient()
    {
        var client = CreateClient();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.Migrate();

        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _keepAlive.Dispose();
        }
    }
}

public sealed class ApiFactory : ApiFactoryBase;
