using MediatR;

namespace UserService.Application.Features.Auth.Commands;
public record CreateSsoUserCommand(
    string Email,
    string Password,
    string Username,
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth
) : IRequest<SuccessResponse>;
