using Application.DTOs.Permissions;
using Domain.Entities;
using Mapster;

namespace Application.Common.Mappings;

/// <summary>
/// Mapster mappings cho Permission, RolePermission, UserPermission.
/// Được scan tự động bởi TypeAdapterConfig.Scan(assembly) trong DependencyInjection.
/// </summary>
public sealed class PermissionMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Permission, PermissionDto>();

        config.NewConfig<RolePermission, RolePermissionDto>()
            .Map(dest => dest.PermissionName,        src => src.Permission.Name)
            .Map(dest => dest.PermissionDisplayName, src => src.Permission.DisplayName)
            .Map(dest => dest.PermissionGroup,       src => src.Permission.Group);

        config.NewConfig<UserPermission, UserPermissionDto>()
            .Map(dest => dest.PermissionName,        src => src.Permission.Name)
            .Map(dest => dest.PermissionDisplayName, src => src.Permission.DisplayName)
            .Map(dest => dest.PermissionGroup,       src => src.Permission.Group);
    }
}
