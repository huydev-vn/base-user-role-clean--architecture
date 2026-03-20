using Application.Interfaces;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
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
                // Hiện câu SQL + giá trị parameter trong log khi dev
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // ── Repositories ──────────────────────────────────────────────────
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();

        // ── Unit of Work ──────────────────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Domain Event Dispatcher ───────────────────────────────────────
        // Dùng MediatR để publish domain events — handlers ở Application nhận và xử lý
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
        // ── Password Hasher ──────────────────────────────────────────────────────────
        // BCrypt việc nhử password và verify — interface ở Application, impl ở đây
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        return services;
    }
}
