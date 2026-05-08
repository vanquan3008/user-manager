using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Domain.Events;

namespace UserService.Application.EventHandlers;
public sealed class UserRegisteredEventEventHandler
    : INotificationHandler<DomainEventNotification<UserRegisteredEvent>>
{
    private readonly ILogger<UserRegisteredEventEventHandler> _logger;

    public UserRegisteredEventEventHandler(ILogger<UserRegisteredEventEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<UserRegisteredEvent> notification, CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        _logger.LogInformation("User registered: UserId={UserId}, Email={Email}, Username={Username}", evt.UserId, evt.Email, evt.Username);
        return Task.CompletedTask;
    }
}
