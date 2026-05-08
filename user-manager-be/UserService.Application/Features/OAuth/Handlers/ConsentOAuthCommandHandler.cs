using MediatR;
using UserService.Application.Features.OAuth.Commands;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;
using UserService.Application.Features.OAuth.Policies;
using UserService.Domain.Constants;

namespace UserService.Application.Features.OAuth.Handlers;

public sealed class ConsentOAuthCommandHandler : IRequestHandler<ConsentOAuthCommand, OAuthAuthorizeResponse>
{
    private readonly IApplicationRepository _applications;
    private readonly IApplicationOAuthScopeRepository _appScopes;
    private readonly IUserOAuthConsentRepository _consents;
    private readonly IPermissionRepository _permissions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IOAuthRedirectUriPolicy _redirectUriPolicy;

    public ConsentOAuthCommandHandler(
        IApplicationRepository applications,
        IApplicationOAuthScopeRepository appScopes,
        IUserOAuthConsentRepository consents,
        IPermissionRepository permissions,
        IOAuthRedirectUriPolicy redirectUriPolicy,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _applications = applications;
        _appScopes = appScopes;
        _consents = consents;
        _permissions = permissions;
        _redirectUriPolicy = redirectUriPolicy;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<OAuthAuthorizeResponse> Handle(ConsentOAuthCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        if (!req.Approve)
            throw new UnauthorizedAccessException("Consent denied by user.");

        var app = await _applications.GetByApiKeyAsync(req.ClientId.Trim(), cancellationToken);
        if (app == null || !app.IsActive)
            throw new BusinessRuleValidationException("OAuthClientInactive", "OAuth client not found or inactive.");

        var redirectUri = req.RedirectUri!.Trim();
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

        // Persist consent (upsert)
        var consent = await _consents.GetActiveConsentAsync(command.UserId, app.Id, cancellationToken);

        var permissionEntities = await _permissions.GetByScopeAsync(requestedScopes, cancellationToken);
        var scopeToId = permissionEntities
            .Where(p => !string.IsNullOrWhiteSpace(p.Scope))
            .ToDictionary(p => p.Scope!, p => p.Id, StringComparer.OrdinalIgnoreCase);

        var missingScopes = requestedScopes.Where(scope => !scopeToId.ContainsKey(scope)).ToArray();
        if (missingScopes.Length > 0)
        {
            var codeEntities = await _permissions.GetByCodeAsync(missingScopes, cancellationToken);
            foreach (var permission in codeEntities)
            {
                scopeToId[permission.Code] = permission.Id;
            }
        }

        var unresolvedScopes = requestedScopes.Where(scope => !scopeToId.ContainsKey(scope)).ToArray();
        if (unresolvedScopes.Length > 0)
            throw new BusinessRuleValidationException("UnknownScopes", $"Unknown scope(s): {string.Join(", ", unresolvedScopes)}");

        var permissionIds = requestedScopes.Select(scope => scopeToId[scope]).ToArray();

        if (consent == null)
        {
            consent = new UserOAuthConsent(Guid.NewGuid(), command.UserId, app.Id);
            consent.ReplaceScopes(permissionIds);
            await _consents.AddAsync(consent, cancellationToken);
        }
        else
        {
            if (!consent.IsActive)
                consent.Activate();
            consent.ReplaceScopes(permissionIds);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Issue auth code after consent
        var authorizeReq = new OAuthAuthorizeRequest(
            ClientId: req.ClientId,
            RedirectUri: redirectUri,
            Scopes: requestedScopes,
            State: req.State,
            CodeChallenge: req.CodeChallenge,
            CodeChallengeMethod: req.CodeChallengeMethod ?? "S256",
            Prompt: null);

        return await _mediator.Send(new AuthorizeOAuthCommand(command.UserId, authorizeReq), cancellationToken);
    }

}

