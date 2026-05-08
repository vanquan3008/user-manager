namespace UserService.Domain.Entities;

public sealed class SharedApplication : Entity<Guid>
{
    private readonly List<SharedApplicationAllowedMember> _allowedMembers = new();

    private SharedApplication() : base() { }

    private SharedApplication(
        Guid id,
        Guid familyPlanId,
        Guid applicationId,
        IEnumerable<Guid> allowedMemberIds,
        DateTime sharedAt)
        : base()
    {
        Id = id;
        FamilyPlanId = familyPlanId;
        ApplicationId = applicationId;
        IsActive = true;
        SharedAt = sharedAt;
        MarkCreated();

        foreach (var memberId in allowedMemberIds ?? Array.Empty<Guid>())
            _allowedMembers.Add(new SharedApplicationAllowedMember(memberId));
    }
    
    public static SharedApplication Create(
        Guid id,
        Guid familyPlanId,
        Guid applicationId,
        IEnumerable<Guid> allowedMemberIds,
        DateTime sharedAt)
    {
        return new SharedApplication(id, familyPlanId, applicationId, allowedMemberIds, sharedAt);
    }

    public Guid FamilyPlanId { get; private set; }
    public Guid ApplicationId { get; private set; }

    public AppApplication Application { get; private set; } = null!;

    public IReadOnlyCollection<SharedApplicationAllowedMember> AllowedMembers => _allowedMembers.AsReadOnly();
    public bool IsActive { get; private set; }
    public DateTime SharedAt { get; private set; }

    public void UpdateAllowedMembers(IEnumerable<Guid> familyMemberIds)
    {
        _allowedMembers.Clear();
        foreach (var id in familyMemberIds ?? Array.Empty<Guid>())
            _allowedMembers.Add(new SharedApplicationAllowedMember(id));
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    public bool IsMemberAllowed(Guid familyMemberId)
    {
        return _allowedMembers.Any(m => m.FamilyMemberId == familyMemberId);
    }
}
