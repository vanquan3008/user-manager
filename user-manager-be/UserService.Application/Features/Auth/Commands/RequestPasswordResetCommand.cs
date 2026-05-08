using MediatR;

namespace UserService.Application.Features.Auth.Commands;

/// <summary>Yêu cầu đặt lại mật khẩu — gửi magic link (forgot password).</summary>
public record RequestPasswordResetCommand(
    string Email
) : IRequest<Unit>;
