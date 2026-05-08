using MediatR;
using UserService.Application.DTOs.OAuth;
using UserService.Application.Features.OAuth.Commands;
using UserService.Application.Mapping;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;
using UserService.Application.Features.OAuth.Policies;
using UserService.Application.Common;

namespace UserService.Application.Features.OAuth.Handlers;

public sealed class AuthorizeOAuthCommandHandler : IRequestHandler<AuthorizeOAuthCommand, OAuthAuthorizeResponse>
{
    private readonly IApplicationRepository _applications;
    private readonly IApplicationOAuthScopeRepository _appScopes;
    private readonly IUserOAuthConsentRepository _consents;
    private readonly IOAuthAuthorizationCodeRepository _codes;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOAuthRedirectUriPolicy _redirectUriPolicy;

    public AuthorizeOAuthCommandHandler(
        IApplicationRepository applications,
        IApplicationOAuthScopeRepository appScopes,
        IUserOAuthConsentRepository consents,
        IOAuthAuthorizationCodeRepository codes,
        IOAuthRedirectUriPolicy redirectUriPolicy,
        IUnitOfWork unitOfWork)
    {
        _applications = applications;
        _appScopes = appScopes;
        _consents = consents;
        _codes = codes;
        _redirectUriPolicy = redirectUriPolicy;
        _unitOfWork = unitOfWork;
    }

    public async Task<OAuthAuthorizeResponse> Handle(AuthorizeOAuthCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request ?? throw new BusinessRuleValidationException("InvalidRequest", "request is required.");

        var clientId = (req.ClientId ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(clientId))
            throw new BusinessRuleValidationException("InvalidClientId", "client_id is required.");

        var redirectUri = (req.RedirectUri ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(redirectUri))
            throw new BusinessRuleValidationException("InvalidRedirectUri", "redirect_uri is required.");

        var codeChallengeMethod = (req.CodeChallengeMethod ?? string.Empty).Trim();
        if (!string.Equals(codeChallengeMethod, "S256", StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleValidationException("InvalidCodeChallengeMethod", "Only PKCE S256 is supported.");

        var codeChallenge = (req.CodeChallenge ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(codeChallenge) || !OAuthHelpers.IsValidBase64Url(codeChallenge))
            throw new BusinessRuleValidationException("InvalidCodeChallenge", "Missing or invalid code_challenge.");

        var app = await _applications.GetByApiKeyAsync(clientId, cancellationToken);
        if (app == null || !app.IsActive)
            throw new BusinessRuleValidationException("OAuthClientInactive", "OAuth client not found or inactive.");

        var allowedUris = string.Join(",", app.AllowedRedirectUris.Select(u => (string)u));
        if (!_redirectUriPolicy.IsAllowed(redirectUri, allowedUris))
            throw new BusinessRuleValidationException("InvalidRedirectUri", "redirect_uri is not allowed.");

        var requestedScopes = (req.Scopes ?? Array.Empty<string>())
            .Select(x => (x ?? string.Empty).Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var allowedScopeCodes = await _appScopes.GetAllowedScopeCodesAsync(app.Id, cancellationToken);
        var allowedDefaultScopes = OAuthScopeNames.FilterDefaultScopes(allowedScopeCodes);
        if (allowedDefaultScopes.Count == 0)
            throw new BusinessRuleValidationException("NoAllowedScopes", "This client has no allowed default scopes configured.");

        if (requestedScopes.Length == 0)
            requestedScopes = allowedDefaultScopes.ToArray();

        var allowedSet = new HashSet<string>(allowedDefaultScopes, StringComparer.OrdinalIgnoreCase);
        var invalidRequested = requestedScopes.Where(s => !allowedSet.Contains(s)).ToArray();
        if (invalidRequested.Length > 0)
            throw new BusinessRuleValidationException("InvalidScopes", $"Invalid scope(s) for this client: {string.Join(", ", invalidRequested)}");

        var consent = await _consents.GetActiveConsentAsync(command.UserId, app.Id, cancellationToken);
        var alreadyGranted = consent == null
            ? Array.Empty<string>()
            : (await _consents.GetGrantedScopesAsync(consent.Id, cancellationToken)).ToArray();

        var grantedSet = new HashSet<string>(alreadyGranted, StringComparer.OrdinalIgnoreCase);
        var missing = requestedScopes.Where(s => !grantedSet.Contains(s)).ToArray();

        var requiresConsent = missing.Length > 0 || string.Equals(req.Prompt, "consent", StringComparison.OrdinalIgnoreCase);
        if (requiresConsent)
        {
            return new OAuthAuthorizeResponse(
                RequiresConsent: true,
                ClientName: app.Name,
                RedirectUri: redirectUri,
                RequestedScopes: requestedScopes,
                AlreadyGrantedScopes: alreadyGranted,
                MissingScopes: missing,
                Code: null,
                State: req.State
            );
        }

        var code = UserService.Application.Common.OAuthHelpers.GenerateCode();
        var scopesString = string.Join(" ", requestedScopes);
        var entity = new OAuthAuthorizationCode(
            Guid.NewGuid(),
            code,
            command.UserId,
            app.Id,
            redirectUri,
            scopesString,
            codeChallenge,
            codeChallengeMethod,
            DateTime.UtcNow.AddMinutes(2));

        await _codes.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OAuthResponseMapper.ToAuthorizeResponse(new OAuthAuthorizeResponseSource(
            false,
            app.Name,
            redirectUri,
            requestedScopes,
            alreadyGranted,
            Array.Empty<string>(),
            code,
            req.State));
    }
}
