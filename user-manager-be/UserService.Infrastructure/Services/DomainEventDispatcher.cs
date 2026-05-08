using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Common;
using UserService.Domain.Entities;
using UserService.Domain.Events;

namespace UserService.Infrastructure.Services;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IMediator mediator, ILogger<DomainEventDispatcher> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents, 
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            _logger.LogDebug("Dispatching domain event: {EventType}", domainEvent.GetType().Name);
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = Activator.CreateInstance(notificationType, domainEvent);
            if (notification is not null)
            {
                await _mediator.Publish(notification, cancellationToken);
            }
        }
    }
}
