using Application.Interfaces;
using Application.Settings;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence.Seeders;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment = false)
    {
        // ── Database ──────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
            );

            if (isDevelopment)
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // ── Health Checks — DB readiness ──────────────────────────────────
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database", tags: ["ready"]);

        // ── Repositories ──────────────────────────────────────────────────
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();

        // ── Unit of Work ──────────────────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Domain Event Dispatcher ───────────────────────────────────────
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

        // ── Services ──────────────────────────────────────────────────────
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // ── Permission Service ────────────────────────────────────────────
        services.AddMemoryCache();
        services.AddScoped<IPermissionService, PermissionService>();

        // ── Settings ──────────────────────────────────────────────────────
        services.Configure<LockoutSettings>(configuration.GetSection("LockoutSettings"));

        // ── Seeders ──────────────────────────────────────────────────────
        services.AddScoped<IPermissionSeeder, PermissionSeeder>();
        services.AddScoped<IUserSeeder, UserSeeder>();

        return services;
    }

    public static async Task InitializeAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        await sp.GetRequiredService<IPermissionSeeder>().SeedAsync();
        await sp.GetRequiredService<IUserSeeder>().SeedAsync();
    }
}
