using UserService.Application.DTOs.Auth;

namespace UserService.Application.Interfaces.Services;

public interface ILoginService
{
    Task<AuthResponse> LoginAsync(LoginUserRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
