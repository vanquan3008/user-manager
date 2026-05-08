using MediatR;

namespace UserService.Application.Features.Auth.Commands;

/// <summary>
/// Resend magic link for email verification or other purposes
/// </summary>
public record ResendMagicLinkCommand(
    string Email,
    string Purpose
) : IRequest<Unit>;
