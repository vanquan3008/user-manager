using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Mapping;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Auth.Handlers;

/// <summary>
/// Handles SSO token exchange: validate that the current SSO user has access to the target app,
/// then issue an app-scoped token. No credentials needed — just a valid SSO session.
/// </summary>
public class SsoExchangeTokenCommandHandler : IRequestHandler<SsoExchangeTokenCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly IUserPermissionService _userPermissionService;
    private readonly IUserApplicationAppService _userApplicationService;
    private readonly IApplicationAppService _applicationService;
    private readonly AppSettings _appSettings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SsoExchangeTokenCommandHandler> _logger;

    public SsoExchangeTokenCommandHandler(
        IUserRepository userRepository,
        IIdentityService identityService,
        ITokenService tokenService,
        IUserPermissionService userPermissionService,
        IUserApplicationAppService userApplicationService,
        IApplicationAppService applicationService,
        AppSettings appSettings,
        IUnitOfWork unitOfWork,
        ILogger<SsoExchangeTokenCommandHandler> logger)
    {
        _userRepository = userRepository;
        _identityService = identityService;
        _tokenService = tokenService;
        _userPermissionService = userPermissionService;
        _userApplicationService = userApplicationService;
        _applicationService = applicationService;
        _appSettings = appSettings;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(SsoExchangeTokenCommand command, CancellationToken cancellationToken)
    {
        var userId = command.UserId;
        if (userId == Guid.Empty)
            throw new InvalidOperationException("UserId is required.");

        var domainUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (domainUser == null)
            throw new InvalidOperationException("User profile not found.");

        var appUser = await _identityService.GetUserByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Identity user not found.");

        // SECURITY: Chặn User bị vô hiệu hoá (banned/deactivated) ở global
        if (!appUser.IsActive)
        {
            _logger.LogWarning("SSO Token Exchange failed: Account is disabled for UserId {UserId}", userId);
            throw new UnauthorizedAccessException("Tài khoản của bạn đã bị vô hiệu hoá.");
        }

        if (!domainUser.IsSSOAccount)
        {
            _logger.LogWarning("SSO Token Exchange failed: Not an SSO account for UserId {UserId}", userId);
            throw new UnauthorizedAccessException("This account is not SSO enabled. Use standard login.");
        }

        var app = await _applicationService.GetByIdAsync(command.TargetApplicationId, cancellationToken);
        if (app == null || !app.IsActive)
        {
            _logger.LogWarning("SSO Token Exchange failed: App {AppId} not found or inactive", command.TargetApplicationId);
            throw new UnauthorizedAccessException("Application not found or inactive.");
        }

        var roles = await _identityService.GetUserRolesAsync(userId, cancellationToken);
        var isSystemAdmin = roles.Any(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) || 
                                          string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                                          string.Equals(r, "SA", StringComparison.OrdinalIgnoreCase));

        if (!isSystemAdmin)
        {
            var membership = await _userApplicationService.GetMembershipAsync(userId, command.TargetApplicationId, cancellationToken);
            if (membership == null || !membership.IsActive)
            {
                _logger.LogWarning("SSO Token Exchange failed: User {UserId} does not have active membership to App {AppId}", userId, command.TargetApplicationId);
                throw new UnauthorizedAccessException($"SSO account does not have access to this application.");
            }
        }

        var permissions = await _userPermissionService.GetPermissionCodesForUserAsync(userId, command.TargetApplicationId, cancellationToken);

        // Generate app-scoped token (with specific application_id) 
        var accessToken = _tokenService.GenerateAccessToken(
            userId,
            appUser.Email,
            appUser.UserName,
            roles,
            permissions,
            applicationId: command.TargetApplicationId,
            scope: null);

        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_appSettings.Jwt.AccessTokenExpirationMinutes);

        // Cập nhật Refresh Token trong DB
        await _identityService.UpdateRefreshTokenAsync(
            userId,
            refreshToken,
            DateTime.UtcNow.AddDays(_appSettings.Jwt.RefreshTokenExpirationDays),
            cancellationToken);

        // Commit UoW (Nếu service UpdateRefreshTokenAsync không tự saveChanges)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully exchanged SSO token for User {UserId} -> App {AppId}", userId, command.TargetApplicationId);

        var userDto = UserDtoMapper.ToDto(domainUser);
        userDto = userDto with
        {
            IsEmailVerified = appUser.EmailConfirmed,
            IsActive = appUser.IsActive,
            Roles = roles
        };

        return new AuthResponse(accessToken, refreshToken, expiresAt, userDto);
    }
}
