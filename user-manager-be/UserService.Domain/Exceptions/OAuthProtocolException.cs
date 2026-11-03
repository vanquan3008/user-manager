namespace UserService.Domain.Exceptions;

/// <summary>An OAuth 2.0 error response (RFC 6749 §5.2) such as <c>invalid_grant</c> or <c>invalid_client</c>.</summary>
public sealed class OAuthProtocolException : DomainException
{
    public string Error { get; }
    public string ErrorDescription { get; }

    public OAuthProtocolException(string error, string description) : base(description)
    {
        Error = error;
        ErrorDescription = description;
    }
}

public static class OAuthErrors
{
    public const string InvalidRequest = "invalid_request";
    public const string InvalidClient = "invalid_client";
    public const string InvalidGrant = "invalid_grant";
    public const string UnauthorizedClient = "unauthorized_client";
    public const string UnsupportedGrantType = "unsupported_grant_type";
    public const string InvalidScope = "invalid_scope";
    public const string AccessDenied = "access_denied";
    public const string InvalidToken = "invalid_token";
}
