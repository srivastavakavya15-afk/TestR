using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestR.Application.Abstractions;
using TestR.Infrastructure.Persistence;
using TestR.Infrastructure.Persistence.Repositories;

namespace TestR.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
