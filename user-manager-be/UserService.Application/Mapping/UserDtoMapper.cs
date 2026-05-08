using UserService.Application.DTOs.Users;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Mapping;

public static class UserDtoMapper
{
    public static UserDto ToDto(User user) => new(
        user.Id.ToString(),
        user.Email.Value,
        user.Name.DisplayName,
        user.Username.Value,
        user.IsEmailVerified,
        !user.IsDeleted,
        Array.Empty<string>(),
        user.CreatedAt);

    public static User FromCreateRequest(CreateUserRequest request, DateTime utcNow) =>
        User.Create(
            Guid.NewGuid(),
            Username.Create(request.Username),
            Email.Create(request.Email),
            PersonName.Create(request.FirstName, request.LastName, request.DisplayName),
            request.DateOfBirth,
            utcNow);

    public static UserWithRolesDto ToUserWithRoles(IdentityUserDto user, IList<string> roles) => new(user, roles);
}
