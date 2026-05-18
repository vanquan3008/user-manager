namespace UserService.Application.DTOs.Users;

public sealed record SSOUserApplicationDto(
    Guid Id,
    string Name,
    string Role
);
