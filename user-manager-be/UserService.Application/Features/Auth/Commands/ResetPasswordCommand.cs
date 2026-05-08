using MediatR;

namespace UserService.Application.Features.Auth.Commands;

public record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword
) : IRequest<Unit>;
