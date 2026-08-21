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
    private static readonly MySqlServerVersion MySqlVersion = new(new Version(8, 0, 46));

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>("Database:Provider") ?? "MySql";
        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Default' is not configured.");
        }

        services.AddDbContext<BetTrackerDbContext>(options =>
        {
            switch (provider)
            {
                case "MySql":
                    options.UseMySql(connectionString, MySqlVersion, mySql =>
                    {
                        mySql.MigrationsAssembly(typeof(BetTrackerDbContext).Assembly.FullName);
                        mySql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                    });
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
