namespace UserService.Application.Interfaces.Services;

public interface IPasswordRecoveryService
{
    Task<bool> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);
}
