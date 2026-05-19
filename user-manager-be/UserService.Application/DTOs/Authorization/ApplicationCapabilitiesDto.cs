namespace UserService.Application.DTOs.Authorization;

public sealed record ApplicationCapabilitiesDto(
    Guid ApplicationId,
    Guid UserId,
    IReadOnlyDictionary<string, bool> Features,
    IReadOnlyDictionary<string, bool> Permissions);
