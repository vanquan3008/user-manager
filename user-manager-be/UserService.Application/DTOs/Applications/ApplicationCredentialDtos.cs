namespace UserService.Application.DTOs.Applications;

public record ApplicationCredentialDto(
    Guid ApplicationId,
    string ApplicationName,
    string ClientId,
    string ClientType,
    string? ClientSecret,
    DateTime? LastClientSecretRotatedAt,
    string? AllowedRedirectUris,
    string? BackchannelLogoutUri);
