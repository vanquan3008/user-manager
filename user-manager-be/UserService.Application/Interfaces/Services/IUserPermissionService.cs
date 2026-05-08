namespace UserService.Application.Interfaces.Services;

/// <summary>
/// Resolves permission codes for a user (from their roles) for JWT and authorization.
/// </summary>
public interface IUserPermissionService
{
    Task<IReadOnlyList<string>> GetPermissionCodesForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetPermissionCodesForUserAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default);
}
