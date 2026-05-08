namespace UserService.Domain.Events;

public sealed record AppApplicationCreatedDomainEvent(
    Guid AppApplicationId, 
    Guid OwnerId, 
    DateTime OccurredOn
) : IDomainEvent;

public sealed record ApiKeyRotatedDomainEvent(
    Guid AppApplicationId, 
    DateTime OccurredOn
) : IDomainEvent;

public sealed record UserAddedToApplicationDomainEvent(
    Guid AppApplicationId, 
    Guid UserId, 
    string Role,
    DateTime OccurredOn
) : IDomainEvent;
