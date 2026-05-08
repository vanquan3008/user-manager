namespace UserService.Application.DTOs.Applications;

public record CreateApplicationResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid OwnerId,
    string? LogoUrl = null,
    string? OwnerDisplayName = null,
    string? AllowedRedirectUris = null,
    string? BackchannelLogoutUri = null);
