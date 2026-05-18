namespace UserService.Application.DTOs.Applications;

public record CreateApplicationResponse(
    Guid Id,
    string Name,
    string ClientId,
    string ClientType,
    string? Description,
    Guid OwnerId,
    string? LogoUrl = null,
    string? OwnerDisplayName = null,
    string? AllowedRedirectUris = null,
    string? BackchannelLogoutUri = null,
    string? ClientSecret = null,
    DateTime? LastClientSecretRotatedAt = null);
