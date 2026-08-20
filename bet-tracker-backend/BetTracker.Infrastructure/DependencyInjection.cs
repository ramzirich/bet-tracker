using BetTracker.Core.Abstractions;
using BetTracker.Infrastructure.Persistence;
using BetTracker.Infrastructure.Security;
using BetTracker.Infrastructure.Services;
using BetTracker.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BetTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>("Database:Provider") ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("Default");

        services.AddDbContext<BetTrackerDbContext>(options =>
        {
            switch (provider)
            {
                case "Sqlite":
                    options.UseSqlite(connectionString);
                    break;
                default:
                    throw new NotSupportedException($"Database provider '{provider}' is not supported.");
            }
        });

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
