namespace UserService.Domain.ValueObjects;
public readonly record struct RoleLookup(Guid Id, string Name, string? Description);
