using UserService.Application.Interfaces.Services;
using UserService.Application.Services;
using UserService.Domain.Repositories;

namespace UserService.Application.Services;

public sealed class PlanApplicationRoleValidator : IPlanApplicationRoleValidator
{
    private readonly IApplicationRoleRepository _applicationRoles;

    public PlanApplicationRoleValidator(IApplicationRoleRepository applicationRoles)
    {
        _applicationRoles = applicationRoles;
    }

    public async Task<bool> AllRoleIdsLinkedToApplicationAsync(
        Guid applicationId,
        IReadOnlyList<Guid> roleIds,
        CancellationToken cancellationToken = default)
    {
        var distinct = roleIds.Where(x => x != Guid.Empty).Distinct().ToList();
        if (distinct.Count == 0)
            return false;

        var count = await _applicationRoles.CountRolesLinkedToApplicationAsync(applicationId, distinct, cancellationToken);
        return count == distinct.Count;
    }
}
