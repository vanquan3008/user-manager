using UserService.Application.DTOs.Auth;

namespace UserService.Application.Interfaces.Services;

public interface IRegistrationService
{
    Task<AuthResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
}
