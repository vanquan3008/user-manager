namespace UserService.Application.Common;
public record ApplicationRoleResult(bool Succeeded, IReadOnlyDictionary<string, string[]>? Errors = null);
