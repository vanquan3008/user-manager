using UserService.Application.Interfaces.Services;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;

namespace UserService.Application.Services;

public sealed class ApplicationMembershipService : IApplicationMembershipService
{
    private readonly IApplicationRepository _applications;
    private readonly IApplicationMembershipRepository _memberships;
    private readonly IApplicationRoleService _roleService;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public ApplicationMembershipService(
        IApplicationRepository applications,
        IApplicationMembershipRepository memberships,
        IApplicationRoleService roleService,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _applications = applications;
        _memberships = memberships;
        _roleService = roleService;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public Task<ApplicationMembership?> GetMembershipAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default)
        => _memberships.GetByUserAndAppAsync(userId, applicationId, cancellationToken);

    public async Task<IReadOnlyList<ApplicationMembership>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var items = await _memberships.GetByUserAsync(userId, cancellationToken);
        return items.ToList();
    }

    public async Task AddUserAsync(
        Guid applicationId,
        Guid userId,
        string? role,
        CancellationToken cancellationToken = default)
    {
        var app = await _applications.GetByIdAsync(applicationId, cancellationToken);
        if (app == null)
            throw new ApplicationNotFoundException(applicationId);

        var existing = await _memberships.GetByUserAndAppAsync(userId, applicationId, cancellationToken);
        if (existing != null)
            throw new UserAlreadyInApplicationException(userId, applicationId);

        var roleName = string.IsNullOrWhiteSpace(role) ? "Guest" : role.Trim();
        var roleId = await _roleService.GetRoleIdForUserAssignmentAsync(applicationId, roleName, cancellationToken);
        if (!roleId.HasValue)
            throw new RoleNotFoundException(roleName, applicationId);

        var membership = ApplicationMembership.Join(
            Guid.NewGuid(),
            userId,
            applicationId,
            roleId.Value,
            _clock.UtcNow);

        await _memberships.AddAsync(membership, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveUserAsync(
        Guid applicationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var membership = await _memberships.GetByUserAndAppAsync(userId, applicationId, cancellationToken);
        if (membership == null)
            return;

        membership.Remove();
        await _memberships.UpdateAsync(membership, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
