using Application.Interfaces;
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
                // Hiện câu SQL + giá trị parameter trong log khi dev
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // ── Repositories ──────────────────────────────────────────────────
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();

        // ── Unit of Work ──────────────────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Domain Event Dispatcher ───────────────────────────────────────
        // Dùng MediatR để publish domain events — handlers ở Application nhận và xử lý
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
        // ── Password Hasher ──────────────────────────────────────────────────────────
        // BCrypt việc nhử password và verify — interface ở Application, impl ở đây
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // ── JWT Token Service ─────────────────────────────────────────────────────────
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // ── Seeders ──────────────────────────────────────────────────────────────────
        // Permissions: seed từ Domain.Constants.Permissions
        services.AddScoped<IPermissionSeeder, PermissionSeeder>();
        // Users: tạo tài khoản Admin/Moderator mặc định (cấu hình trong DefaultAccounts)
        services.AddScoped<IUserSeeder, UserSeeder>();

        // ── Permission Service ────────────────────────────────────────────────────────
        // Tính effective permissions + in-memory cache (TTL 5 phút)
        services.AddMemoryCache();
        services.AddScoped<IPermissionService, PermissionService>();

        return services;
    }

    /// <summary>
    /// Khởi tạo dữ liệu mặc định (seed permissions) khi application startup.
    /// Gọi sau khi <see cref="WebApplication"/> đã được build.
    /// </summary>
    public static async Task InitializeAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        // Thứ tự quan trọng: permissions trước, sau đó users
        await sp.GetRequiredService<IPermissionSeeder>().SeedAsync();
        await sp.GetRequiredService<IUserSeeder>().SeedAsync();
    }
}
