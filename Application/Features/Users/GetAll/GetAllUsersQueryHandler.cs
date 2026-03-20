using Application.Common;
using Application.Common.Messaging;
using Application.DTOs.Users;
using Domain.Interfaces.Repositories;
using MapsterMapper;

namespace Application.Features.Users.GetAll;

internal sealed class GetAllUsersQueryHandler(
    IUserRepository userRepository,
    IMapper mapper)
    : IQueryHandler<GetAllUsersQuery, PagedResult<UserDto>>
{
    public async Task<Result<PagedResult<UserDto>>> Handle(
        GetAllUsersQuery query,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await userRepository.GetPagedAsync(
            query.Page,
            query.PageSize,
            query.Search,
            query.Role,
            query.Status,
            cancellationToken);

        var result = new PagedResult<UserDto>
        {
            Items = items.Select(u => mapper.Map<UserDto>(u)),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };

        return Result<PagedResult<UserDto>>.Success(result);
    }
}
