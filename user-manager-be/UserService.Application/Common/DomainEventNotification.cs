using MediatR;
using UserService.Domain.Events;

namespace UserService.Application.Common;

public sealed record DomainEventNotification<T>(T DomainEvent) : INotification
    where T : IDomainEvent;
