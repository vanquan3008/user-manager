using MediatR;

namespace UserService.Application.Features.Subscriptions.Commands;

public record AddSubscriptionMemberCommand(
    Guid SubscriptionId,
    Guid ApplicationId,
    Guid UserId,
    Guid RoleTypeId) : IRequest<Unit>;

public record RemoveSubscriptionMemberCommand(
    Guid SubscriptionId,
    Guid ApplicationId,
    Guid UserId) : IRequest<Unit>;
