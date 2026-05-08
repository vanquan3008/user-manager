namespace UserService.Application.DTOs.Users;

public sealed record SsoUserApplicationDto(
    Guid Id,
    string Name,
    string Role
);
