using API.Extensions;
using API.Middleware;
using API.Services;
using Application;
using Application.Interfaces;
using Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting application...");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // ── Services ─────────────────────────────────────────────────────────
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.IsDevelopment());

    builder.Services.AddControllers();
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddPermissionAuthorization();
    builder.Services.AddCorsPolicy(builder.Configuration);
    builder.Services.AddSwaggerWithJwt();
    builder.Services.AddApiRateLimiting();
    builder.Services.AddApiHealthChecks();

    // ── Pipeline ─────────────────────────────────────────────────────────
    var app = builder.Build();

    await app.Services.InitializeAsync();

    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseStaticFiles();

    if (app.Environment.IsDevelopment())
        app.UseSwaggerWithUI();

    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapApiHealthChecks();

    app.Run();
}
catch (HostAbortedException) { }
catch (Exception ex) { Log.Fatal(ex, "Application terminated unexpectedly."); }
finally { Log.CloseAndFlush(); }
