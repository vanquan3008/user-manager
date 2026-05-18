namespace UserService.Application.Interfaces.Services;

public interface IEmailVerificationService
{
    Task<bool> VerifyEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default);
    Task<bool> VerifyEmailWithTokenAsync(string token, CancellationToken cancellationToken = default);
}
