namespace UserService.Application.Interfaces.Services;

public interface IMagicLinkDeliveryService
{
    Task<bool> ResendMagicLinkAsync(
        string email,
        string purpose,
        CancellationToken cancellationToken = default);

    Task<bool> RequestPasswordlessLoginAsync(
        string email,
        Guid? applicationId = null,
        CancellationToken cancellationToken = default);
}
