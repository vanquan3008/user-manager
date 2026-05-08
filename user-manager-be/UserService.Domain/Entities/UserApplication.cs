namespace UserService.Domain.Entities;

public sealed class UserApplication : AggregateRoot<Guid>
{
    private UserApplication() : base() { }

    private UserApplication(
        Guid id,
        Guid userId,
        Guid applicationId,
        string role,
        Guid? licensePackageId = null)
        : base()
    {
        Id = id;
        UserId = userId;
        ApplicationId = applicationId;
        LicensePackageId = licensePackageId;
        IsActive = true;
        MarkCreated();
        Role = role;
        MergePermissionsFromRole(role);
    }
    
    public static UserApplication Create(
        Guid id,
        Guid userId,
        Guid applicationId,
        string role,
        Guid? licensePackageId = null)
    {
        return new UserApplication(id, userId, applicationId, role, licensePackageId);
    }

    public Guid UserId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public Guid? LicensePackageId { get; private set; }

    public User User { get; private set; } = null!;
    public AppApplication Application { get; private set; } = null!;
    public LicensePackage? LicensePackage { get; private set; }

    public string Role { get; private set; } = string.Empty;
    public string? LicenseAssignedRole { get; private set; }
    
    public bool CanRead { get; private set; }
    public bool CanWrite { get; private set; }
    public bool CanDelete { get; private set; }
    public bool CanManageUsers { get; private set; }
    public bool CanManageSubscription { get; private set; }
    
    public bool IsActive { get; private set; }
    public DateTime? InvitedAt { get; private set; }
    public DateTime? JoinedAt { get; private set; }

    public void UpdateRole(string newRole)
    {
        Role = newRole;
        MergePermissionsFromRole(Role);
        Touch();
    }

    public void ApplyLicenseRole(string? licenseRole, Guid? licensePackageId)
    {
        LicenseAssignedRole = licenseRole;
        LicensePackageId = licensePackageId;
        
        if (!string.IsNullOrEmpty(licenseRole))
        {
            Role = licenseRole;
        }
        
        MergePermissionsFromRole(Role);
        Touch();
    }

    public void ClearLicenseRole()
    {
        LicenseAssignedRole = null;
        LicensePackageId = null;
        Touch();
    }

    public void SetLicensePackage(Guid? licensePackageId)
    {
        LicensePackageId = licensePackageId;
        Touch();
    }

    public void UpdatePermissions(bool canRead, bool canWrite, bool canDelete, bool canManageUsers, bool canManageSubscription)
    {
        CanRead = canRead;
        CanWrite = canWrite;
        CanDelete = canDelete;
        CanManageUsers = canManageUsers;
        CanManageSubscription = canManageSubscription;
        Touch();
    }

    public void Activate(DateTime joinedAt)
    {
        IsActive = true;
        if (!JoinedAt.HasValue)
            JoinedAt = joinedAt;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    public void SetInvited(DateTime invitedAt)
    {
        InvitedAt = invitedAt;
        Touch();
    }

    private void MergePermissionsFromRole(string role)
    {
        CanRead = false;
        CanWrite = false;
        CanDelete = false;
        CanManageUsers = false;
        CanManageSubscription = false;

        switch (role)
        {
            case "Admin":
            case "SuperAdmin":
                CanRead = true;
                CanWrite = true;
                CanDelete = true;
                CanManageUsers = true;
                CanManageSubscription = true;
                break;
            case "Developer":
                CanRead = true;
                CanWrite = true;
                CanDelete = true;
                break;
            case "Editor":
                CanRead = true;
                CanWrite = true;
                break;
            case "Viewer":
                CanRead = true;
                break;
        }
    }
}
