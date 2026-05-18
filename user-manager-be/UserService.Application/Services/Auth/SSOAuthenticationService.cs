using Microsoft.Extensions.Logging;
using UserService.Application.Configuration;
using UserService.Application.DTOs.Auth;
using UserService.Application.DTOs.OAuth;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Application.Mapping;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.Services.Auth;

public sealed class SSOAuthenticationService : ISSOAuthenticationService
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUserPermissionService _userPermissionService;
    private readonly IApplicationAccessService _applicationAccessService;
    private readonly IApplicationAppService _applicationService;
    private readonly IOAuthFlowService _oauthFlowService;
    private readonly AppSettings _appSettings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AuthResponseFactory _authResponseFactory;
    private readonly ILogger<SSOAuthenticationService> _logger;

    public SSOAuthenticationService(
        IIdentityService identityService,
        IUserRepository userRepository,
        ITokenService tokenService,
        IUserPermissionService userPermissionService,
        IApplicationAccessService applicationAccessService,
        IApplicationAppService applicationService,
        IOAuthFlowService oauthFlowService,
        AppSettings appSettings,
        IUnitOfWork unitOfWork,
        AuthResponseFactory authResponseFactory,
        ILogger<SSOAuthenticationService> logger)
    {
        _identityService = identityService;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _userPermissionService = userPermissionService;
        _applicationAccessService = applicationAccessService;
        _applicationService = applicationService;
        _oauthFlowService = oauthFlowService;
        _appSettings = appSettings;
        _unitOfWork = unitOfWork;
        _authResponseFactory = authResponseFactory;
        _logger = logger;
    }

    public async Task<AuthResponse> SSOLoginAsync(
        SSOLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var (succeeded, appUser, errors) = await _identityService.LoginAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (!succeeded || appUser == null)
            throw new UnauthorizedException(string.Join(", ", errors));

        var domainUser = await _userRepository.GetByIdAsync(appUser.Id, cancellationToken);
        if (domainUser == null)
            throw new EntityNotFoundException(nameof(User), appUser.Id);

        if (!domainUser.IsSSOAccount)
            throw new UnauthorizedException("This account is not SSO enabled.");

        return await _authResponseFactory.BuildSSOAuthResponseAsync(appUser.Id, cancellationToken);
    }

    public async Task<SSOAuthorizeResponse> AuthorizeAsync(
        SSOAuthorizeCommand command,
        CancellationToken cancellationToken = default)
    {
        var (appUser, _) = await ValidateSSOCredentialsAsync(command.Email, command.Password, cancellationToken);

        var response = await _oauthFlowService.AuthorizeAsync(
            appUser.Id,
            new OAuthAuthorizeRequest(
                ClientId: command.ClientId,
                RedirectUri: command.RedirectUri,
                Scopes: command.Scopes,
                State: command.State,
                CodeChallenge: command.CodeChallenge ?? string.Empty,
                CodeChallengeMethod: command.CodeChallengeMethod ?? "S256"),
            cancellationToken);

        return ToSSOAuthorizeResponse(response);
    }

    public async Task<SSOAuthorizeResponse> ConsentAsync(
        SSOConsentCommand command,
        CancellationToken cancellationToken = default)
    {
        var (appUser, _) = await ValidateSSOCredentialsAsync(command.Email, command.Password, cancellationToken);

        var response = await _oauthFlowService.ConsentAsync(
            appUser.Id,
            new OAuthConsentRequest(
                ClientId: command.ClientId,
                RedirectUri: command.RedirectUri,
                Scopes: command.Scopes,
                State: command.State,
                CodeChallenge: command.CodeChallenge ?? string.Empty,
                CodeChallengeMethod: command.CodeChallengeMethod ?? "S256",
                Approve: command.Approve),
            cancellationToken);

        return ToSSOAuthorizeResponse(response);
    }

    public async Task<AuthResponse> ExchangeTokenAsync(
        SSOExchangeTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.UserId == Guid.Empty)
            throw new InvalidOperationException("UserId is required.");

        var domainUser = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (domainUser == null)
            throw new InvalidOperationException("User profile not found.");

        var appUser = await _identityService.GetUserByIdAsync(command.UserId, cancellationToken)
            ?? throw new InvalidOperationException("Identity user not found.");

        if (!appUser.IsActive)
        {
            _logger.LogWarning("SSO Token Exchange failed: Account is disabled for UserId {UserId}", command.UserId);
            throw new UnauthorizedAccessException("Your account has been disabled.");
        }

        if (!domainUser.IsSSOAccount)
        {
            _logger.LogWarning("SSO Token Exchange failed: Not an SSO account for UserId {UserId}", command.UserId);
            throw new UnauthorizedAccessException("This account is not SSO enabled. Use standard login.");
        }

        var app = await _applicationService.GetByIdAsync(command.TargetApplicationId, cancellationToken);
        if (app == null || !app.IsActive)
        {
            _logger.LogWarning("SSO Token Exchange failed: App {AppId} not found or inactive", command.TargetApplicationId);
            throw new UnauthorizedAccessException("Application not found or inactive.");
        }

        var roles = await _identityService.GetUserRolesAsync(command.UserId, cancellationToken);
        var hasAccess = await _applicationAccessService.HasAccessAsync(
            command.UserId,
            command.TargetApplicationId,
            roles,
            cancellationToken);
        if (!hasAccess)
        {
            _logger.LogWarning(
                "SSO Token Exchange failed: User {UserId} does not have active access to App {AppId}",
                command.UserId,
                command.TargetApplicationId);
            throw new UnauthorizedAccessException("SSO account does not have access to this application.");
        }

        var permissions = await _userPermissionService.GetPermissionCodesForUserAsync(
            command.UserId,
            command.TargetApplicationId,
            cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(
            new AccessTokenRequest(
                command.UserId,
                appUser.Email,
                appUser.UserName,
                roles,
                permissions,
                command.TargetApplicationId,
                Scope: null));

        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_appSettings.Jwt.AccessTokenExpirationMinutes);
        await _identityService.UpdateRefreshTokenAsync(
            command.UserId,
            refreshToken,
            DateTime.UtcNow.AddDays(_appSettings.Jwt.RefreshTokenExpirationDays),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully exchanged SSO token for User {UserId} -> App {AppId}",
            command.UserId,
            command.TargetApplicationId);

        var userDto = UserDtoMapper.ToDto(domainUser) with
        {
            IsEmailVerified = appUser.EmailConfirmed,
            IsActive = appUser.IsActive,
            Roles = roles
        };

        return new AuthResponse(accessToken, refreshToken, expiresAt, userDto);
    }

    private async Task<(IdentityUserDto User, User DomainUser)> ValidateSSOCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var (succeeded, appUser, errors) = await _identityService.LoginAsync(email, password, cancellationToken);
        if (!succeeded || appUser == null)
            throw new UnauthorizedException(string.Join(", ", errors));

        var domainUser = await _userRepository.GetByIdAsync(appUser.Id, cancellationToken);
        if (domainUser == null)
            throw new EntityNotFoundException(nameof(User), appUser.Id);

        if (!domainUser.IsSSOAccount)
            throw new UnauthorizedException("This account is not SSO enabled.");

        return (appUser, domainUser);
    }

    private static SSOAuthorizeResponse ToSSOAuthorizeResponse(OAuthAuthorizeResponse response)
        => new(
            RequiresConsent: response.RequiresConsent,
            Code: response.Code,
            RedirectUri: response.RedirectUri,
            State: response.State,
            ClientName: response.ClientName,
            RequestedScopes: response.RequestedScopes,
            MissingScopes: response.MissingScopes);
}
