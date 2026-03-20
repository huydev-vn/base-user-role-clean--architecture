using Application.Common.Messaging;

namespace Application.Features.Users.UpdateProfile;

public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? AvatarUrl
) : ICommand;
