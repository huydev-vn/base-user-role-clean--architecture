using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace API.Extensions;

public static class HealthChecksExtensions
{
    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services)
    {
        // DB check được đăng ký trong Infrastructure.DependencyInjection qua AddDbContextCheck
        services.AddHealthChecks();
        return services;
    }

    public static IEndpointRouteBuilder MapApiHealthChecks(this IEndpointRouteBuilder app)
    {
        // /health — liveness: app đang chạy (không check dependency)
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        // /health/ready — readiness: app + DB ready nhận traffic
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        return app;
    }
}
