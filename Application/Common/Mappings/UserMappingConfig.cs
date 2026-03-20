using Application.DTOs.Users;
using Domain.Entities;
using Mapster;

namespace Application.Common.Mappings;

/// <summary>
/// Mapster config tập trung cho User mappings.
/// Được Scan tự động bởi TypeAdapterConfig.Scan(assembly) trong DependencyInjection.
/// </summary>
public sealed class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}".Trim());
    }
}
