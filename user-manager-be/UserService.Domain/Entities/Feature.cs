using UserService.Domain.Exceptions;

namespace UserService.Domain.Entities;

/// <summary>
/// Product capability enabled by subscription plans. Features do not grant permissions.
/// </summary>
public sealed class Feature : AuditableAggregateRoot<Guid>
{
    private Feature() : base() { }

    private Feature(Guid id, string code, string name, string description, bool isSystem)
        : base()
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Code = NormalizeCode(code);
        Name = NormalizeRequired(name, nameof(name));
        Description = description?.Trim() ?? string.Empty;
        IsSystem = isSystem;
        IsActive = true;
        MarkCreated();
    }

    public static Feature Create(Guid id, string code, string name, string description, bool isSystem = true)
        => new(id, code, name, description, isSystem);

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; }

    public void Rename(string name, string description)
    {
        Name = NormalizeRequired(name, nameof(name));
        Description = description?.Trim() ?? string.Empty;
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    private static string NormalizeCode(string code)
    {
        var normalized = NormalizeRequired(code, nameof(code)).ToUpperInvariant();
        if (normalized.Contains('.') || normalized.Contains(' '))
            throw new InvalidDomainObjectException(nameof(code), code, "Feature code must be an uppercase module code, e.g. USER_MANAGEMENT.");

        return normalized;
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidDomainObjectException(fieldName, value, $"{fieldName} is required.");

        return value.Trim();
    }
}
