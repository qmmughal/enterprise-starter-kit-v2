namespace EnterpriseKit.Infrastructure.DependencyInjection;

using EnterpriseKit.Domain.Interfaces.Repositories;
using EnterpriseKit.Infrastructure.Outbox;
using EnterpriseKit.Infrastructure.Persistence;
using EnterpriseKit.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── EF Core with Npgsql ──────────────────────────────────────────
        services.AddDbContext<ApplicationDbContext>(opts =>
        {
            opts.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                });

            // Enable sensitive data logging only in development
            // opts.EnableSensitiveDataLogging(); // uncomment locally if needed
        });

        // Expose DbContext to TransactionBehaviour in Application layer
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // ── Repositories ─────────────────────────────────────────────────
        services.AddScoped<IOrderRepository, OrderRepository>();

        // ── Outbox relay ─────────────────────────────────────────────────
        services.AddHostedService<OutboxRelayService>();

        return services;
    }
}
