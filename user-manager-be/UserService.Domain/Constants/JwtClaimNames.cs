namespace UserService.Domain.Constants;

public static class JwtClaimNames
{
    public const string UserId = "userId";
    public const string Permissions = "permissions";
    public const string ApplicationId = "application_id";
    public const string Scope = "scope";
    public const string SSOAppIds = "sso_app_ids";
    public const string OAuthScopes = "oauth_scopes";
    /// <summary>Server-side session id (OIDC <c>sid</c>); used for revocation and single logout.</summary>
    public const string SessionId = "sid";
    public const string ClientId = "client_id";
    public const string AuthenticationMethod = "amr";
}
