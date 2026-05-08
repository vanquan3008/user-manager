namespace UserService.Application.Interfaces.Services;

public interface IApplicationOAuthScopeAppService
{
    /// <summary>
    /// Gets the allowed OAuth scope codes for a specific application
    /// </summary>
    Task<IReadOnlyCollection<string>> GetAllowedScopeCodesAsync(Guid applicationId, CancellationToken cancellationToken = default);
}
