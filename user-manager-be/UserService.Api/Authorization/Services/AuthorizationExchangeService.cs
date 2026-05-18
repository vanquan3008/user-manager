using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using UserService.Api.Authorization.Validation;
using UserService.Api.Extensions;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Constants;
using UserService.Infrastructure.Identity;

namespace UserService.Api.Authorization.Services;
public sealed class AuthorizationExchangeService : IAuthorizationExchangeService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IMagicLinkService _magicLinkService;
    private readonly IValidator<PasswordGrantInput> _passwordGrantValidator;
    private readonly IValidator<MagicLinkGrantInput> _magicLinkGrantValidator;
    private readonly IUserPermissionService _userPermissionService;
    private readonly IApplicationAccessService _applicationAccessService;
    private readonly IApplicationAppService _applicationService;
    private readonly IApplicationOAuthScopeAppService _applicationOAuthScopeService;

    public AuthorizationExchangeService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IMagicLinkService magicLinkService,
        IValidator<PasswordGrantInput> passwordGrantValidator,
        IValidator<MagicLinkGrantInput> magicLinkGrantValidator,
        IUserPermissionService userPermissionService,
        IApplicationAccessService applicationAccessService,
        IApplicationAppService applicationService,
        IApplicationOAuthScopeAppService applicationOAuthScopeService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _magicLinkService = magicLinkService;
        _passwordGrantValidator = passwordGrantValidator;
        _magicLinkGrantValidator = magicLinkGrantValidator;
        _userPermissionService = userPermissionService;
        _applicationAccessService = applicationAccessService;
        _applicationService = applicationService;
        _applicationOAuthScopeService = applicationOAuthScopeService;
    }

    public async Task<IActionResult> ExchangeAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        var request = httpContext.GetRequiredOpenIddictServerRequest();

        if (request.IsPasswordGrantType())
            return await ExchangePasswordGrantAsync(httpContext, request, cancellationToken);

        if (request.IsRefreshTokenGrantType())
            return await ExchangeRefreshTokenGrantAsync(httpContext, cancellationToken);

        if (request.IsAuthorizationCodeGrantType())
            return await ExchangeAuthorizationCodeGrantAsync(httpContext, cancellationToken);

        if (string.Equals(request.GrantType, OAuthGrantTypes.MagicLink, StringComparison.Ordinal))
            return await ExchangeMagicLinkGrantAsync(request, cancellationToken);

        return OAuthInvalidGrant("The specified grant type is not implemented.");
    }

    private async Task<IActionResult> ExchangePasswordGrantAsync(HttpContext httpContext, OpenIddictRequest request, CancellationToken cancellationToken)
    {
        var input = new PasswordGrantInput(
            request.Username ?? request.GetParameter(OAuthParameterNames.Username)?.ToString(),
            request.Password ?? request.GetParameter(OAuthParameterNames.Password)?.ToString());

        var validation = await _passwordGrantValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
            return OAuthInvalidGrant(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage).Distinct()));

        var username = input.Username!;
        var password = input.Password!;

        var user = await _userManager.FindByEmailAsync(username)
            ?? await _userManager.FindByNameAsync(username);
        if (user == null)
            return OAuthInvalidGrant("Invalid credentials.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return OAuthInvalidGrant("Invalid credentials.");

        var applicationId = ParseOptionalGuid(request.GetParameter(OAuthParameterNames.ApplicationId)?.ToString());
        if (request.GetParameter(OAuthParameterNames.ApplicationId) is not null && !applicationId.HasValue)
            return OAuthInvalidGrant("application_id must be a valid GUID.");

        return await IssueSignInResultAsync(user, applicationId, cancellationToken);
    }

    private async Task<IActionResult> ExchangeRefreshTokenGrantAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var result = await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (!result.Succeeded)
            return OAuthInvalidGrant(result.Failure?.Message ?? "Invalid refresh token.");

        var userId = result.Principal!.GetClaim(ClaimTypes.NameIdentifier) ??
                     result.Principal!.GetClaim(OpenIddictConstants.Claims.Subject);
        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null)
            return OAuthInvalidGrant("User no longer exists.");

        var applicationId = ParseOptionalGuid(result.Principal!.FindFirst(JwtClaimNames.ApplicationId)?.Value);
        var oauthScopes = ReadCsvClaim(result.Principal!, JwtClaimNames.OAuthScopes);
        return await IssueSignInResultAsync(user, applicationId, cancellationToken, oauthScopes);
    }

    private async Task<IActionResult> ExchangeAuthorizationCodeGrantAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var result = await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (!result.Succeeded)
            return OAuthInvalidGrant(result.Failure?.Message ?? "Invalid authorization code.");

        var userId = result.Principal!.GetClaim(ClaimTypes.NameIdentifier) ??
                     result.Principal!.GetClaim(OpenIddictConstants.Claims.Subject);
        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null)
            return OAuthInvalidGrant("User no longer exists.");

        var applicationId = ParseOptionalGuid(result.Principal!.FindFirst(JwtClaimNames.ApplicationId)?.Value);
        var oauthScopes = ReadCsvClaim(result.Principal!, JwtClaimNames.OAuthScopes);
        return await IssueSignInResultAsync(user, applicationId, cancellationToken, oauthScopes);
    }

    private async Task<IActionResult> ExchangeMagicLinkGrantAsync(OpenIddictRequest request, CancellationToken cancellationToken)
    {
        var input = new MagicLinkGrantInput(
            request.GetParameter(OAuthParameterNames.Token)?.ToString(),
            request.GetParameter(OAuthParameterNames.ApplicationId)?.ToString());
        var validation = await _magicLinkGrantValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
            return OAuthInvalidGrant(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage).Distinct()));

        var token = input.Token!;
        var (isValid, userId) = await _magicLinkService.ValidateMagicLinkAsync(
            token,
            EmailPurpose.MagicLinkPurpose.PasswordlessLogin,
            cancellationToken);
        if (!isValid || !userId.HasValue)
            return OAuthInvalidGrant("Invalid or expired magic link.");

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null)
            return OAuthInvalidGrant("User no longer exists.");

        return await IssueSignInResultAsync(user, input.ParseApplicationId(), cancellationToken);
    }

    private async Task<ClaimsIdentity> CreateUserIdentityAsync(
        AppUser user,
        Guid? applicationId,
        CancellationToken cancellationToken,
        IReadOnlyCollection<string>? requestedOauthScopes = null)
    {
        var identity = new ClaimsIdentity(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role);

        identity.SetClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString());
        identity.SetClaim(OpenIddictConstants.Claims.Name, user.UserName ?? user.Email);
        identity.SetClaim(OpenIddictConstants.Claims.Email, user.Email);
        identity.SetClaim(ClaimTypes.NameIdentifier, user.Id.ToString());

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
            identity.AddClaim(OpenIddictConstants.Claims.Role, role);

        IReadOnlyList<string> permissions;
        IReadOnlyCollection<string>? oauthScopes = requestedOauthScopes;

        if (applicationId.HasValue)
        {
            // Use application service instead of direct repository access
            var application = await _applicationService.GetByIdAsync(applicationId.Value, cancellationToken);
            if (application == null || !application.IsActive)
                throw new InvalidOperationException("Application not found or inactive.");

            var hasAccess = await _applicationAccessService.HasAccessAsync(
                user.Id,
                applicationId.Value,
                roles,
                cancellationToken);
            if (!hasAccess)
                throw new InvalidOperationException("User does not have active access to this application.");

            permissions = await _userPermissionService.GetPermissionCodesForUserAsync(user.Id, applicationId.Value, cancellationToken);

            if (oauthScopes == null)
            {
                // Use application service instead of direct repository access
                var allowedScopes = await _applicationOAuthScopeService.GetAllowedScopeCodesAsync(applicationId.Value, cancellationToken);
                var filteredScopes = OAuthScopeNames.FilterDefaultScopes(allowedScopes);
                oauthScopes = filteredScopes.Count == 0 ? null : filteredScopes;
            }
            else
            {
                var filteredScopes = OAuthScopeNames.FilterDefaultScopes(oauthScopes);
                oauthScopes = filteredScopes.Count == 0 ? null : filteredScopes;
            }

            identity.SetClaim(JwtClaimNames.ApplicationId, applicationId.Value.ToString());
        }
        else
        {
            permissions = await _userPermissionService.GetPermissionCodesForUserAsync(user.Id, cancellationToken);
        }

        if (permissions.Count > 0)
            identity.SetClaim(JwtClaimNames.Permissions, string.Join(",", permissions.Distinct(StringComparer.OrdinalIgnoreCase)));

        if (oauthScopes != null && oauthScopes.Count > 0)
        {
            // oauthScopes already contains the allowed OAuth2 scope names (e.g., "profile", "email")
            // No need to intersect with permissions - just use them directly
            identity.SetClaim(JwtClaimNames.OAuthScopes, string.Join(",", oauthScopes.Distinct(StringComparer.OrdinalIgnoreCase)));
        }

        identity.SetDestinations(claim => claim.Type switch
        {
            OpenIddictConstants.Claims.Name or OpenIddictConstants.Claims.Email => new[] { OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken },
            OpenIddictConstants.Claims.Role => new[] { OpenIddictConstants.Destinations.AccessToken },
            _ => new[] { OpenIddictConstants.Destinations.AccessToken }
        });

        return identity;
    }

    private static IActionResult OAuthInvalidGrant(string description)
        => new BadRequestObjectResult(new { error = "invalid_grant", error_description = description });

    private async Task<IActionResult> IssueSignInResultAsync(
        AppUser user,
        Guid? applicationId,
        CancellationToken cancellationToken,
        IReadOnlyCollection<string>? requestedOauthScopes = null)
    {
        try
        {
            var identity = await CreateUserIdentityAsync(user, applicationId, cancellationToken, requestedOauthScopes);
            var principal = new ClaimsPrincipal(identity);
            return new Microsoft.AspNetCore.Mvc.SignInResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, principal);
        }
        catch (InvalidOperationException ex)
        {
            return OAuthInvalidGrant(ex.Message);
        }
    }

    private static Guid? ParseOptionalGuid(string? value)
        => Guid.TryParse(value, out var applicationId) ? applicationId : null;

    private static IReadOnlyCollection<string> ReadCsvClaim(ClaimsPrincipal principal, string claimType)
    {
        var claimValue = principal.FindFirst(claimType)?.Value;
        if (string.IsNullOrWhiteSpace(claimValue))
            return Array.Empty<string>();

        return claimValue
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
