using MediatR;

namespace UserService.Application.Features.Subscriptions.Queries;

public record GetActiveSubscriptionQuery(Guid UserId, Guid ApplicationId) : IRequest<SubscriptionDto?>;

public record GetSubscriptionHistoryQuery(Guid UserId, Guid ApplicationId) : IRequest<IEnumerable<SubscriptionDto>>;
