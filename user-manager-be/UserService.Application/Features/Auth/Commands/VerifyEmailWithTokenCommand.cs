using MediatR;

namespace UserService.Application.Features.Auth.Commands;

/// <summary>Verify email khi user click link trong email (chỉ cần token).</summary>
public record VerifyEmailWithTokenCommand(string Token) : IRequest<bool>;
