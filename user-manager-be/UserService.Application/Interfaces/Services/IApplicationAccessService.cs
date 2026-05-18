namespace UserService.Application.Interfaces.Services;

using UserService.Application.Common;

public interface IApplicationAccessService
{
    Task<ApplicationAccessEvaluation> EvaluateAsync(
        Guid userId,
        Guid applicationId,
        IEnumerable<string>? roleNames = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasAccessAsync(
        Guid userId,
        Guid applicationId,
        IEnumerable<string>? roleNames = null,
        CancellationToken cancellationToken = default);

    Task EnsureAccessAsync(
        Guid userId,
        Guid applicationId,
        IEnumerable<string>? roleNames = null,
        CancellationToken cancellationToken = default);
}
