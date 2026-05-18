namespace UserService.Application.Common;

public sealed record ApplicationAccessEvaluation(
    bool HasSystemRoleAccess,
    bool HasActiveMembership,
    bool HasActiveSubscription)
{
    public bool HasAccess => HasSystemRoleAccess || (HasActiveMembership && HasActiveSubscription);
}
