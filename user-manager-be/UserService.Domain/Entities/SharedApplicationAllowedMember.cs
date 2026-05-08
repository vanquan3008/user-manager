namespace UserService.Domain.Entities;

public sealed class SharedApplicationAllowedMember
{
    public Guid FamilyMemberId { get; private set; }

    public FamilyMember FamilyMember { get; private set; } = null!;

    private SharedApplicationAllowedMember() { }

    internal SharedApplicationAllowedMember(Guid familyMemberId)
    {
        FamilyMemberId = familyMemberId;
    }
}
