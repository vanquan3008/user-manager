using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Common;
using UserService.Domain.Events;

namespace UserService.Application.EventHandlers;
public sealed class EmailVerifiedEventEventHandler
    : INotificationHandler<DomainEventNotification<EmailVerifiedEvent>>
{
    private readonly ILogger<EmailVerifiedEventEventHandler> _logger;

    public EmailVerifiedEventEventHandler(ILogger<EmailVerifiedEventEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<EmailVerifiedEvent> notification, CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        _logger.LogInformation("Email verified: UserId={UserId}, Email={Email}", evt.UserId, evt.Email);
        return Task.CompletedTask;
    }
}
