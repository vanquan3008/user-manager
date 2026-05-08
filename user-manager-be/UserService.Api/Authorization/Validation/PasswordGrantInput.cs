namespace UserService.Api.Authorization.Validation;

public sealed record PasswordGrantInput(string? Username, string? Password);

