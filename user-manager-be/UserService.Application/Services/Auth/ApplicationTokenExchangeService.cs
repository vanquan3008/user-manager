using Microsoft.Extensions.Logging;
using UserService.Application.Configuration;
using UserService.Application.DTOs.Auth;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Application.Mapping;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;

namespace UserService.Application.Services.Auth;

public sealed class ApplicationTokenExchangeService : IApplicationTokenExchangeService
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUserPermissionService _userPermissionService;
    private readonly IApplicationAccessService _applicationAccessService;
    private readonly IApplicationAppService _applicationService;
    private readonly AppSettings _appSettings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<ApplicationTokenExchangeService> _logger;

    public ApplicationTokenExchangeService(
        IIdentityService identityService,
        IUserRepository userRepository,
        ITokenService tokenService,
        IUserPermissionService userPermissionService,
        IApplicationAccessService applicationAccessService,
        IApplicationAppService applicationService,
        AppSettings appSettings,
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<ApplicationTokenExchangeService> logger)
    {
        _identityService = identityService;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _userPermissionService = userPermissionService;
        _applicationAccessService = applicationAccessService;
        _applicationService = applicationService;
        _appSettings = appSettings;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<AuthResponse> ExchangeTokenAsync(
        ExchangeApplicationTokenCommand command,
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
            _logger.LogWarning("Application token exchange failed: Account is disabled for UserId {UserId}", command.UserId);
            throw new UnauthorizedAccessException("Your account has been disabled.");
        }

        if (!domainUser.IsSSOAccount)
        {
            _logger.LogWarning("Application token exchange failed: Not an SSO-managed account for UserId {UserId}", command.UserId);
            throw new UnauthorizedAccessException("This account is not provisioned for managed application token exchange.");
        }

        var app = await _applicationService.GetByIdAsync(command.TargetApplicationId, cancellationToken);
        if (app == null || !app.IsActive)
        {
            _logger.LogWarning("Application token exchange failed: App {AppId} not found or inactive", command.TargetApplicationId);
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
                "Application token exchange failed: User {UserId} does not have active access to App {AppId}",
                command.UserId,
                command.TargetApplicationId);
            throw new UnauthorizedAccessException("User does not have active access to this application.");
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
        var utcNow = _clock.UtcNow;
        var expiresAt = utcNow.AddMinutes(_appSettings.Jwt.AccessTokenExpirationMinutes);
        await _identityService.UpdateRefreshTokenAsync(
            command.UserId,
            refreshToken,
            utcNow.AddDays(_appSettings.Jwt.RefreshTokenExpirationDays),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully exchanged application token for User {UserId} -> App {AppId}",
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
}
