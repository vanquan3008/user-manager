namespace UserService.Domain.ValueObjects;

public sealed record PermissionCode
{
    public string Value { get; }

    private PermissionCode(string value) => Value = value;

    public static PermissionCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Permission code cannot be empty.", nameof(value));

        var trimmed = value.Trim();

        if (!trimmed.Contains('.'))
            throw new ArgumentException("Permission code must be in format 'resource.action', e.g., 'users.create'.", nameof(value));

        if (trimmed.Length > 100)
            throw new ArgumentException("Permission code cannot exceed 100 characters.", nameof(value));
        return new PermissionCode(trimmed);
    }

    public static implicit operator string(PermissionCode code) => code.Value;
}
