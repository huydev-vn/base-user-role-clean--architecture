namespace Infrastructure.Persistence.Seeders;

/// <summary>
/// Interface cho permission seeding — dùng để DI trong application.
/// </summary>
public interface IPermissionSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
