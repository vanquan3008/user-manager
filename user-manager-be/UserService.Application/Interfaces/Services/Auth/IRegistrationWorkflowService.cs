using UserService.Application.DTOs.Auth;

namespace UserService.Application.Interfaces.Services;

public interface IRegistrationWorkflowService
{
    Task<Guid> CreateUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
}
