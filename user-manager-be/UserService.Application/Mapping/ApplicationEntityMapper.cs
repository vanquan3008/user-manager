using UserService.Application.DTOs.Applications;
using UserService.Application.DTOs.Auth;
using UserService.Domain.Entities;

namespace UserService.Application.Mapping;

public static class ApplicationEntityMapper
{
    public static CreateApplicationResponse ToCreateApplicationResponse(AppApplication app, User? ownerFallback = null)
    {
        var owner = app.Owner ?? ownerFallback;
        return new(
            app.Id,
            app.Name.Value,
            app.ApiKey.Value,
            app.ClientType,
            string.IsNullOrEmpty(app.Description.Value) ? null : app.Description.Value,
            app.OwnerId,
            app.LogoUrl?.Value,
            ResolveOwnerDisplayName(owner),
            string.Join(",", app.AllowedRedirectUris.Select(u => (string)u)),
            app.BackchannelLogoutUri?.Value,
            null,
            app.LastApiKeyRotatedAt);
    }

    public static ApplicationCredentialDto ToCredentialDto(AppApplication app, string? plainClientSecret = null)
        => new(
            app.Id,
            app.Name.Value,
            app.ApiKey.Value,
            app.ClientType,
            plainClientSecret,
            app.LastApiKeyRotatedAt,
            string.Join(",", app.AllowedRedirectUris.Select(u => (string)u)),
            app.BackchannelLogoutUri?.Value);

    private static string? ResolveOwnerDisplayName(User? owner)
    {
        if (owner is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(owner.Name?.DisplayName))
        {
            return owner.Name.DisplayName.Trim();
        }

        return owner.Email?.Value;
    }

    public static ApplicationUserDto ToApplicationUserDto(ApplicationMembership membership) =>
        new(
            membership.UserId, 
            membership.RoleId.ToString(), 
            membership.User?.Email?.Value, 
            membership.User?.Username?.Value ?? membership.User?.Name?.DisplayName);

}
