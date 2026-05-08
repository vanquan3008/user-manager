using MediatR;

namespace UserService.Application.Features.Auth.Commands;

/// <summary>Đổi mật khẩu (user đã đăng nhập, biết mật khẩu hiện tại).</summary>
public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<Unit>;
