using MediatR;

namespace UserService.Application.Features.Auth.Commands;
public record RegisterUserCommand(
    string Email,
    string Password,
    string Username,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    Guid? ApplicationId = null
) : IRequest<AuthResponse>;
