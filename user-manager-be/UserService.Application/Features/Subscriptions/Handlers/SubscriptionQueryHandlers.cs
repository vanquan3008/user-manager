using MediatR;
using UserService.Application.DTOs.Subscriptions;
using UserService.Application.Features.Subscriptions.Queries;
using UserService.Application.Mapping;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Subscriptions.Handlers;

public class GetActiveSubscriptionQueryHandler : IRequestHandler<GetActiveSubscriptionQuery, SubscriptionDto?>
{
    private readonly ISubscriptionRepository _repository;

    public GetActiveSubscriptionQueryHandler(ISubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<SubscriptionDto?> Handle(GetActiveSubscriptionQuery query, CancellationToken cancellationToken)
    {
        var subscription = await _repository.GetActiveSubscriptionAsync(
            query.UserId,
            query.ApplicationId,
            cancellationToken);
        return subscription == null ? null : SubscriptionEntityMapper.ToDto(subscription);
    }
}

public class GetSubscriptionHistoryQueryHandler : IRequestHandler<GetSubscriptionHistoryQuery, IEnumerable<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _repository;

    public GetSubscriptionHistoryQueryHandler(ISubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<SubscriptionDto>> Handle(GetSubscriptionHistoryQuery query, CancellationToken cancellationToken)
    {
        var subscriptions = await _repository.GetUserSubscriptionsAsync(
            query.UserId,
            query.ApplicationId,
            cancellationToken);
        return subscriptions.Select(SubscriptionEntityMapper.ToDto).ToList();
    }
}
