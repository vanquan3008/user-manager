using MediatR;
using UserService.Application.Common;
using UserService.Application.Configuration;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Services;
using UserService.Application.Mapping;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.Events;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;
using static UserService.Domain.Constants.EmailPurpose;

namespace UserService.Application.Services;

public class AuthService : IAuthServices
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly IMagicLinkService _magicLinkService;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;
    private readonly IUserPermissionService _userPermissionService;
    private readonly IApplicationAppService _applicationService;
    private readonly IUserApplicationAppService _userApplicationService;
    private readonly IMediator _mediator;
    private readonly AppSettings _appSettings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public AuthService(
        IIdentityService identityService,
        IUserRepository userRepository,
        IMagicLinkService magicLinkService,
        IEmailService emailService,
        ITokenService tokenService,
        IUserPermissionService userPermissionService,
        IApplicationAppService applicationService,
        IUserApplicationAppService userApplicationService,
        IMediator mediator,
        IUnitOfWork unitOfWork,
        IClock clock,
        AppSettings appSettings)
    {
        _identityService = identityService;
        _userRepository = userRepository;
        _magicLinkService = magicLinkService;
        _emailService = emailService;
        _tokenService = tokenService;
        _userPermissionService = userPermissionService;
        _applicationService = applicationService;
        _userApplicationService = userApplicationService;
        _mediator = mediator;
        _appSettings = appSettings;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<AuthResponse> RegisterUserAsync(
        string email,
        string password,
        string username,
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        Guid? applicationId = null,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await _identityService.GetUserByEmailAsync(email, cancellationToken);
        if (existingUser != null)
        {
            if (!existingUser.EmailConfirmed)
            {
                await ResendMagicLinkAsync(email, MagicLinkPurpose.EmailVerification, cancellationToken);
                throw new VerificationEmailResentException("Email is not confirmed. A new verification email has been sent.");
            }
            throw new DuplicateUserException("email", email);
        }

        AppApplication? application = null;
        if (applicationId.HasValue)
        {
            application = await _applicationService.GetByIdAsync(applicationId.Value, cancellationToken);
            if (application == null || !application.IsActive)
                throw new InvalidOperationException("Application not found or inactive.");
        }

        var (succeeded, userId, errors) = await _identityService.RegisterAsync(
            email, password, username, cancellationToken);
        if (!succeeded)
            throw new InvalidOperationException($"Registration failed: {string.Join(", ", errors)}");
        try
        {
            var user = CreateDomainUser(userId!.Value, username, email, firstName, lastName, dateOfBirth);

            await _userRepository.AddAsync(user, cancellationToken);
            await _identityService.UpdateUserProfileIdAsync(userId.Value, user.Id, cancellationToken);

            if (application != null)
            {
                application.AddUser(userId.Value, "Viewer", _clock.UtcNow);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(
                new DomainEventNotification<UserRegisteredEvent>(
                    new UserRegisteredEvent(userId.Value, email, username)),
                cancellationToken);
        }
        catch
        {
            await _identityService.DeleteUserAsync(userId!.Value, cancellationToken);
            throw;
        }

        var roles = await _identityService.GetUserRolesAsync(userId.Value, cancellationToken);
        var permissions = applicationId.HasValue
            ? await _userPermissionService.GetPermissionCodesForUserAsync(userId.Value, applicationId.Value, cancellationToken)
            : await _userPermissionService.GetPermissionCodesForUserAsync(userId.Value, cancellationToken);
        var accessToken = applicationId.HasValue
            ? _tokenService.GenerateAccessToken(userId.Value, email, username, roles, permissions, applicationId)
            : _tokenService.GenerateAccessToken(userId.Value, email, username, roles, permissions);

        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = _clock.UtcNow.AddMinutes(_appSettings.Jwt.AccessTokenExpirationMinutes);

        await _identityService.UpdateRefreshTokenAsync(
            userId.Value, refreshToken,
            _clock.UtcNow.AddDays(_appSettings.Jwt.RefreshTokenExpirationDays), 
            cancellationToken);

        var domainUser = await _userRepository.GetByIdAsync(userId.Value, cancellationToken)
            ?? throw new InvalidOperationException("User profile not found after registration");

        var userDto = UserDtoMapper.ToDto(domainUser) with
        {
            Roles = roles,
            IsEmailVerified = false,
            IsActive = true
        };
        return new AuthResponse(accessToken, refreshToken, expiresAt, userDto, RequiresEmailVerification: true);
    }

    private User CreateDomainUser(Guid userId, string username, string email, string firstName, string lastName, DateOnly dateOfBirth)
    {
        var usernameVO = Username.Create(username);
        var emailVO = Email.Create(email);
        var nameVO = PersonName.Create(firstName, lastName);
        return User.Create(userId, usernameVO, emailVO, nameVO, dateOfBirth, _clock.UtcNow);
    }

    public async Task<AuthResponse> LoginUserAsync(
        string email,
        string password,
        Guid? applicationId = null,
        string? returnUrl = null,
        CancellationToken cancellationToken = default)
    {
        var (succeeded, appUser, errors) = await _identityService.LoginAsync(email, password, cancellationToken);

        if (!succeeded || appUser == null)
            throw new UnauthorizedAccessException(string.Join(", ", errors));
        var domainUser = await _userRepository.GetByIdAsync(appUser.Id, cancellationToken);
        if (domainUser == null)
            throw new InvalidOperationException("User profile not found");
        if (domainUser.IsSSOAccount)
            throw new SsoLoginRequiredException(email, applicationId, returnUrl);
        var roles = await _identityService.GetUserRolesAsync(appUser.Id, cancellationToken);
        var isAdmin = roles.Contains(RoleNames.Admin, StringComparer.OrdinalIgnoreCase)
            || roles.Contains(RoleNames.SuperAdmin, StringComparer.OrdinalIgnoreCase);

        if (!isAdmin && applicationId.HasValue)
        {
            var membership = await _userApplicationService.GetMembershipAsync(appUser.Id, applicationId.Value, cancellationToken);
            if (membership == null || !membership.IsActive)
                throw new UnauthorizedAccessException("User does not belong to this application.");
        }

        var permissions = applicationId.HasValue && !isAdmin
            ? await _userPermissionService.GetPermissionCodesForUserAsync(appUser.Id, applicationId.Value, cancellationToken)
            : await _userPermissionService.GetPermissionCodesForUserAsync(appUser.Id, cancellationToken);

        var accessToken = applicationId.HasValue
            ? _tokenService.GenerateAccessToken(appUser.Id, appUser.Email, appUser.UserName, roles, permissions, applicationId)
            : _tokenService.GenerateAccessToken(appUser.Id, appUser.Email, appUser.UserName, roles, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var expiresAt = _clock.UtcNow.AddMinutes(_appSettings.Jwt.AccessTokenExpirationMinutes);
        var refreshTokenExpiresAt = _clock.UtcNow.AddDays(_appSettings.Jwt.RefreshTokenExpirationDays);

        await _identityService.UpdateRefreshTokenAsync(
            appUser.Id, 
            refreshToken,
            refreshTokenExpiresAt,
            cancellationToken
        );

        var userDto = UserDtoMapper.ToDto(domainUser);
        userDto = userDto with
        {
            IsEmailVerified = appUser.EmailConfirmed,
            IsActive = appUser.IsActive,
            Roles = roles
        };

        return new AuthResponse(accessToken, refreshToken, expiresAt, userDto);
    }

    public async Task<AuthResponse> SsoLoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var (succeeded, appUser, errors) = await _identityService.LoginAsync(email, password, cancellationToken);
        if (!succeeded || appUser == null)
            throw new UnauthorizedAccessException(string.Join(", ", errors));

        var domainUser = await _userRepository.GetByIdAsync(appUser.Id, cancellationToken);
        if (domainUser == null)
            throw new InvalidOperationException("User profile not found");

        if (!domainUser.IsSSOAccount)
            throw new UnauthorizedAccessException("This account is not SSO enabled.");

        return await BuildSsoAuthResponseAsync(appUser.Id, appUser.Email, appUser.UserName, appUser.EmailConfirmed, appUser.IsActive, domainUser, cancellationToken);
    }

    private async Task<AuthResponse> BuildSsoAuthResponseAsync(
        Guid userId,
        string email,
        string userName,
        bool emailConfirmed,
        bool isActive,
        User domainUser,
        CancellationToken cancellationToken)
    {
        var roles = await _identityService.GetUserRolesAsync(userId, cancellationToken);
        var permissions = await _userPermissionService.GetPermissionCodesForUserAsync(userId, cancellationToken);

        var isSystemAdmin = roles.Any(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) || 
                                          string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                                          string.Equals(r, "SA", StringComparison.OrdinalIgnoreCase));

        var userApps = await _userApplicationService.GetByUserAsync(userId, cancellationToken);
        var activeUserApps = userApps.Where(ua => ua.IsActive).ToList();
        var ssoAppIds = activeUserApps.Select(ua => ua.ApplicationId).Distinct().ToList();

        if (!isSystemAdmin && ssoAppIds.Count == 0)
            throw new UnauthorizedAccessException("SSO account has no authorized applications. Contact an administrator.");

        var applicationInfos = new List<ApplicationInfoDto>();
        foreach (var ua in activeUserApps)
        {
            var app = await _applicationService.GetByIdAsync(ua.ApplicationId, cancellationToken);
            if (app != null && app.IsActive)
            {
                applicationInfos.Add(new ApplicationInfoDto(
                    app.Id,
                    app.Name,
                    app.Description,
                    ua.Role,
                    ua.CanRead,
                    ua.CanWrite,
                    ua.CanDelete,
                    ua.CanManageUsers,
                    ua.CanManageSubscription));
            }
        }


        var accessToken = _tokenService.GenerateAccessToken(
            userId,
            email,
            userName,
            roles,
            permissions,
            applicationId: null,
            scope: null,
            ssoAppIds: ssoAppIds);

        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = _clock.UtcNow.AddMinutes(_appSettings.Jwt.AccessTokenExpirationMinutes);
        var refreshTokenExpiresAt = _clock.UtcNow.AddDays(_appSettings.Jwt.RefreshTokenExpirationDays);

        await _identityService.UpdateRefreshTokenAsync(
            userId,
            refreshToken,
            refreshTokenExpiresAt,
            cancellationToken);

        var userDto = UserDtoMapper.ToDto(domainUser);
        userDto = userDto with
        {
            IsEmailVerified = emailConfirmed,
            IsActive = isActive,
            Roles = roles
        };

        return new AuthResponse(accessToken, refreshToken, expiresAt, userDto,
            RequiresEmailVerification: false,
            Applications: applicationInfos);
    }

    public async Task<AuthResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new UnauthorizedAccessException("Invalid refresh token");
        var appUser = await _identityService.GetUserByRefreshTokenAsync(refreshToken, cancellationToken);
        if (appUser == null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        var domainUser = await _userRepository.GetByIdAsync(appUser.Id, cancellationToken);
        if (domainUser?.IsSSOAccount == true)
        {
            return await BuildSsoAuthResponseAsync(
                appUser.Id, appUser.Email, appUser.UserName,
                appUser.EmailConfirmed, appUser.IsActive,
                domainUser, cancellationToken);
        }
        return await GetAuthResponseForUserAsync(appUser.Id, cancellationToken);
    }

    public async Task<bool> VerifyEmailAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken = default)
    {
        var result = await _identityService.VerifyEmailAsync(userId, token, cancellationToken);
        if (!result)
            return false;
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            user.VerifyEmail();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var appUser = await _identityService.GetUserByIdAsync(userId, cancellationToken);
            if (appUser != null)
            {
                await _emailService.SendWelcomeEmailAsync(appUser.Email, appUser.UserName, cancellationToken);
            }
        }

        return true;
    }

    public async Task<bool> VerifyEmailWithTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var (isValid, userId) = await _magicLinkService.ValidateMagicLinkAsync(
            token, MagicLinkPurpose.EmailVerification, cancellationToken);
        if (!isValid || !userId.HasValue)
            return false;

        var confirmed = await _identityService.ConfirmEmailAsync(userId.Value, cancellationToken);
        if (!confirmed)
            return false;

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user != null)
        {
            user.VerifyEmail();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var appUser = await _identityService.GetUserByIdAsync(userId.Value, cancellationToken);
            if (appUser != null)
                await _emailService.SendWelcomeEmailAsync(appUser.Email, appUser.UserName, cancellationToken);
        }

        return true;
    }

    public async Task<bool> VerifyMagicLinkAsync(
        string token,
        string purpose,
        CancellationToken cancellationToken = default)
    {
        var (isValid, _) = await _magicLinkService.ValidateMagicLinkAsync(token, purpose, cancellationToken);
        return isValid;
    }

    public async Task<AuthResponse> GetAuthResponseForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await BuildAuthResponseForUserAsync(userId, null, cancellationToken);

    private async Task<AuthResponse> BuildAuthResponseForUserAsync(
        Guid userId,
        Guid? applicationId,
        CancellationToken cancellationToken = default)
    {
        var appUser = await _identityService.GetUserByIdAsync(userId, cancellationToken);
        if (appUser == null)
            throw new UnauthorizedAccessException("User not found");

        var domainUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (domainUser == null)
            throw new InvalidOperationException("User profile not found");

        var roles = await _identityService.GetUserRolesAsync(userId, cancellationToken);
        var isAdmin = roles.Contains(RoleNames.Admin, StringComparer.OrdinalIgnoreCase)
            || roles.Contains(RoleNames.SuperAdmin, StringComparer.OrdinalIgnoreCase);

        var permissions = applicationId.HasValue && !isAdmin
            ? await _userPermissionService.GetPermissionCodesForUserAsync(userId, applicationId.Value, cancellationToken)
            : await _userPermissionService.GetPermissionCodesForUserAsync(userId, cancellationToken);

        if (applicationId.HasValue && !isAdmin)
        {
            var membership = await _userApplicationService.GetMembershipAsync(userId, applicationId.Value, cancellationToken);
            if (membership == null || !membership.IsActive)
                throw new UnauthorizedAccessException("User does not belong to this application.");
        }

        var accessToken = applicationId.HasValue
            ? _tokenService.GenerateAccessToken(userId, appUser.Email, appUser.UserName, roles, permissions, applicationId)
            : _tokenService.GenerateAccessToken(userId, appUser.Email, appUser.UserName, roles, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = _clock.UtcNow.AddMinutes(_appSettings.Jwt.AccessTokenExpirationMinutes);
        await _identityService.UpdateRefreshTokenAsync(
            userId,
            refreshToken,
            _clock.UtcNow.AddDays(_appSettings.Jwt.RefreshTokenExpirationDays),
            cancellationToken);

        var userDto = UserDtoMapper.ToDto(domainUser);
        userDto = userDto with
        {
            IsEmailVerified = appUser.EmailConfirmed,
            IsActive = appUser.IsActive,
            Roles = roles
        };

        return new AuthResponse(accessToken, refreshToken, expiresAt, userDto);
    }

    public async Task<bool> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await _identityService.GetUserByEmailAsync(email, cancellationToken);
        
        if (user == null)
            return true; // Don't reveal if email exists

        var token = await _magicLinkService.GenerateMagicLinkAsync(
            user.Id,
            MagicLinkPurpose.PasswordReset,
            email,
            cancellationToken: cancellationToken);

        var baseUrl = GetMagicLinkBaseUrl(MagicLinkPurpose.PasswordReset);
        var magicLink = _magicLinkService.BuildMagicLinkUrl(token, MagicLinkPurpose.PasswordReset, baseUrl, email);

        await _emailService.SendPasswordResetEmailAsync(email, magicLink, user.UserName, cancellationToken);

        return true;
    }

    public async Task<bool> ResendMagicLinkAsync(
        string email,
        string purpose,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var user = await _identityService.GetUserByEmailAsync(email, cancellationToken);
        
        if (user == null)
            return false;

        var token = await _magicLinkService.GenerateMagicLinkAsync(
            user.Id,
            purpose,
            email,
            cancellationToken: cancellationToken);

        var baseUrl = GetMagicLinkBaseUrl(purpose);
        var magicLink = _magicLinkService.BuildMagicLinkUrl(token, purpose, baseUrl, email);

        await _emailService.SendMagicLinkEmailAsync(
            email,
            magicLink,
            user.UserName,
            purpose,
            cancellationToken);

        return true;
    }

    public async Task<bool> RequestPasswordlessLoginAsync(
        string email,
        Guid? applicationId = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _identityService.GetUserByEmailAsync(email, cancellationToken);
        
        if (user == null)
            return true; 
        var token = await _magicLinkService.GenerateMagicLinkAsync(
            user.Id,
            MagicLinkPurpose.PasswordlessLogin,
            email,
            cancellationToken: cancellationToken);
        var baseUrl = GetMagicLinkBaseUrl(MagicLinkPurpose.PasswordlessLogin);
        var clientId = await ResolveClientIdAsync(applicationId, cancellationToken);
        var magicLink = _magicLinkService.BuildMagicLinkUrl(
            token,
            MagicLinkPurpose.PasswordlessLogin,
            baseUrl,
            clientId: clientId);
        await _emailService.SendMagicLinkEmailAsync(
            email,
            magicLink,
            user.UserName,
            MagicLinkPurpose.PasswordlessLogin,
            cancellationToken);

        return true;
    }

    public async Task<AuthResponse> VerifyMagicLinkAndAuthenticateAsync(
        string token,
        string purpose,
        Guid? applicationId = null,
        CancellationToken cancellationToken = default)
    {
        var (isValid, userId) = await _magicLinkService.ValidateMagicLinkAsync(
            token,
            purpose,
            cancellationToken);

        if (!isValid || !userId.HasValue)
            throw new UnauthorizedAccessException("Invalid or expired magic link");

        return await BuildAuthResponseForUserAsync(userId.Value, applicationId, cancellationToken);
    }

    private string GetMagicLinkBaseUrl(string purpose)
    {
        var url = purpose switch
        {
            MagicLinkPurpose.PasswordReset => _appSettings.FrontendBaseUrl,
            MagicLinkPurpose.PasswordlessLogin => _appSettings.FrontendBaseUrl,
            MagicLinkPurpose.EmailVerification => _appSettings.FrontendBaseUrl,
            _ => _appSettings.BaseUrl
        };

        return !string.IsNullOrWhiteSpace(url)? url.Trim() : throw new InvalidOperationException($"Base URL is not configured for purpose '{purpose}'.");
    }

    private async Task<string?> ResolveClientIdAsync(Guid? applicationId, CancellationToken cancellationToken)
    {
        if (!applicationId.HasValue)
            return null;

        var application = await _applicationService.GetByIdAsync(applicationId.Value, cancellationToken);
        if (application == null || !application.IsActive)
            throw new InvalidOperationException("Application not found or inactive.");

        return application.ApiKey;
    }
}
