namespace UserService.Application.Interfaces.Services;

public interface IPlanApplicationRoleValidator
{
    Task<bool> AllRoleIdsLinkedToApplicationAsync(Guid applicationId, IReadOnlyList<Guid> roleIds, CancellationToken cancellationToken = default);
}
