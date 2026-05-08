using MediatR;
using UserService.Application.Common;
using UserService.Application.Configuration;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Domain.Events;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;

namespace UserService.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMagicLinkService> _magicLinkServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IUserPermissionService> _userPermissionServiceMock;
    private readonly Mock<IApplicationAppService> _applicationAppServiceMock;
    private readonly Mock<IUserApplicationAppService> _userApplicationAppServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IClock> _clockMock;
    private readonly AppSettings _appSettings;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _magicLinkServiceMock = new Mock<IMagicLinkService>();
        _emailServiceMock = new Mock<IEmailService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _userPermissionServiceMock = new Mock<IUserPermissionService>();
        _applicationAppServiceMock = new Mock<IApplicationAppService>();
        _userApplicationAppServiceMock = new Mock<IUserApplicationAppService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mediatorMock = new Mock<IMediator>();
        _clockMock = new Mock<IClock>();
        _clockMock.SetupGet(x => x.UtcNow).Returns(new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc));
        _appSettings = new AppSettings
        {
            BaseUrl = "https://test.local",
            FrontendBaseUrl = "https://test.local",
            Jwt = new JwtConfiguration
            {
                SecretKey = "test-secret-key-min-32-chars-long",
                Issuer = "Test",
                Audience = "Test",
                AccessTokenExpirationMinutes = 60,
                RefreshTokenExpirationDays = 7
            }
        };
        _sut = new AuthService(
            _identityServiceMock.Object,
            _userRepositoryMock.Object,
            _magicLinkServiceMock.Object,
            _emailServiceMock.Object,
            _tokenServiceMock.Object,
            _userPermissionServiceMock.Object,
            _applicationAppServiceMock.Object,
            _userApplicationAppServiceMock.Object,
            _mediatorMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object,
            _appSettings);
    }

    [Fact]
    public async Task RegisterUserAsync_WhenEmailAlreadyExists_ThrowsDuplicateUserException()
    {
        var email = "existing@test.com";
        _identityServiceMock
            .Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUserDto(
                Guid.NewGuid(), "user", email, true, true, false, null, null, null, DateTime.UtcNow, null));

        var act = () => _sut.RegisterUserAsync(
            email, "Pass123!", "user1", "First", "Last", new DateOnly(1990, 1, 1));

        await act.Should().ThrowAsync<UserService.Domain.Exceptions.DuplicateUserException>();
    }

    [Fact]
    public async Task RegisterUserAsync_WhenEmailExistsButNotConfirmed_ResendsMagicLinkAndThrowsVerificationEmailResentException()
    {
        var userId = Guid.NewGuid();
        var email = "unconfirmed@test.com";
        _identityServiceMock
            .Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUserDto(userId, "user", email, false, true, false, null, null, null, DateTime.UtcNow, null));
        _magicLinkServiceMock
            .Setup(x => x.GenerateMagicLinkAsync(userId, "EmailVerification", email, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("token123");
        _magicLinkServiceMock
            .Setup(x => x.BuildMagicLinkUrl("token123", "EmailVerification", "https://test.local", It.IsAny<string?>(), null))
            .Returns("https://test.local/verify?token=token123");

        var act = () => _sut.RegisterUserAsync(
            email, "Pass123!", "user1", "First", "Last", new DateOnly(1990, 1, 1));

        await act.Should().ThrowAsync<VerificationEmailResentException>();
        _emailServiceMock.Verify(
            x => x.SendMagicLinkEmailAsync(email, It.IsAny<string>(), "user", "EmailVerification", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_WhenRegistrationSucceeds_ReturnsAuthResponseWithRequiresEmailVerification()
    {
        var userId = Guid.NewGuid();
        var email = "new@test.com";
        var username = "newuser";
        _identityServiceMock.Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync((IdentityUserDto?)null);
        _identityServiceMock
            .Setup(x => x.RegisterAsync(email, It.IsAny<string>(), username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, userId, Array.Empty<string>()));
        _identityServiceMock
            .Setup(x => x.GetUserRolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "User" });
        _identityServiceMock
            .Setup(x => x.UpdateRefreshTokenAsync(userId, It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _identityServiceMock
            .Setup(x => x.UpdateUserProfileIdAsync(userId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var domainUser = CreateUser(userId, username, email, "First", "Last");
        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(domainUser);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(domainUser);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(userId, email, username, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>())).Returns("access");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh");

        var result = await _sut.RegisterUserAsync(
            email, "Pass123!", username, "First", "Last", new DateOnly(1990, 1, 1));

        result.Should().NotBeNull();
        result.RequiresEmailVerification.Should().BeTrue();
        result.AccessToken.Should().Be("access");
        result.RefreshToken.Should().Be("refresh");
        result.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task LoginUserAsync_WhenCredentialsInvalid_ThrowsUnauthorizedAccessException()
    {
        _identityServiceMock
            .Setup(x => x.LoginAsync("bad@test.com", "wrong", It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, (IdentityUserDto?)null, new[] { "Invalid credentials" }));

        var act = () => _sut.LoginUserAsync("bad@test.com", "wrong");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid credentials*");
    }

    [Fact]
    public async Task LoginUserAsync_WhenUserProfileNotFound_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        _identityServiceMock
            .Setup(x => x.LoginAsync("u@test.com", "Pass1!", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, new IdentityUserDto(userId, "u", "u@test.com", true, true, false, null, null, null, DateTime.UtcNow, null), Array.Empty<string>()));
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var act = () => _sut.LoginUserAsync("u@test.com", "Pass1!");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*User profile not found*");
    }

    [Fact]
    public async Task LoginUserAsync_WhenValid_ReturnsAuthResponse()
    {
        var userId = Guid.NewGuid();
        var email = "u@test.com";
        var domainUser = CreateUser(userId, "user", email, "F", "L", new DateOnly(1995, 5, 5));
        _identityServiceMock
            .Setup(x => x.LoginAsync(email, "Pass1!", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, new IdentityUserDto(userId, "u", email, true, true, false, null, null, null, DateTime.UtcNow, null), Array.Empty<string>()));
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(domainUser);
        _identityServiceMock.Setup(x => x.GetUserRolesAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<string> { "User" });
        _identityServiceMock.Setup(x => x.UpdateRefreshTokenAsync(userId, It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(userId, email, "u", It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>())).Returns("access");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh");

        var result = await _sut.LoginUserAsync(email, "Pass1!");

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access");
        result.RefreshToken.Should().Be("refresh");
        result.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task LoginUserAsync_WhenAccountIsSso_ThrowsSsoLoginRequiredException()
    {
        var userId = Guid.NewGuid();
        var email = "sso@test.com";
        var domainUser = CreateUser(userId, "sso_user", email, "F", "L", new DateOnly(1995, 5, 5));
        domainUser.EnableSSO();

        _identityServiceMock
            .Setup(x => x.LoginAsync(email, "Pass1!", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, new IdentityUserDto(userId, "sso_user", email, true, true, false, null, null, null, DateTime.UtcNow, null), Array.Empty<string>()));
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(domainUser);

        var act = () => _sut.LoginUserAsync(email, "Pass1!", null, "/oauth/authorize?client_id=test");

        var ex = await act.Should().ThrowAsync<SsoLoginRequiredException>();
        ex.Which.Email.Should().Be(email);
        ex.Which.ReturnUrl.Should().Be("/oauth/authorize?client_id=test");
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenNullOrEmpty_ThrowsUnauthorizedAccessException()
    {
        var act = () => _sut.RefreshTokenAsync("");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid refresh token*");
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenValidToken_ReturnsAuthResponse()
    {
        var refreshToken = "valid-refresh-token";
        var userId = Guid.NewGuid();
        var domainUser = CreateUser(userId, "user", "u@test.com", "First", "Last");
        _identityServiceMock
            .Setup(x => x.GetUserByRefreshTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUserDto(userId, "user", "u@test.com", true, true, false, null, null, null, DateTime.UtcNow, null));
        _identityServiceMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new IdentityUserDto(userId, "user", "u@test.com", true, true, false, null, null, null, DateTime.UtcNow, null));
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(domainUser);
        _identityServiceMock.Setup(x => x.GetUserRolesAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<string> { "User" });
        _userPermissionServiceMock.Setup(x => x.GetPermissionCodesForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<string>());
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>())).Returns("new-access");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("new-refresh");
        _identityServiceMock.Setup(x => x.UpdateRefreshTokenAsync(userId, It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _sut.RefreshTokenAsync(refreshToken);

        result.AccessToken.Should().Be("new-access");
        result.RefreshToken.Should().Be("new-refresh");
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenIdentityReturnsFalse_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        _identityServiceMock
            .Setup(x => x.VerifyEmailAsync(userId, "bad-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.VerifyEmailAsync(userId, "bad-token");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenIdentityReturnsTrue_UpdatesDomainUserAndReturnsTrue()
    {
        var userId = Guid.NewGuid();
        var domainUser = CreateUser(userId, "user", "u@test.com", "F", "L");
        _identityServiceMock
            .Setup(x => x.VerifyEmailAsync(userId, "valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(domainUser);
        _identityServiceMock
            .Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUserDto(userId, "u", "u@test.com", true, true, false, null, null, null, DateTime.UtcNow, null));

        var result = await _sut.VerifyEmailAsync(userId, "valid-token");

        result.Should().BeTrue();
        domainUser.IsEmailVerified.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(x => x.SendWelcomeEmailAsync("u@test.com", "u", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyMagicLinkAsync_WhenValid_ReturnsTrue()
    {
        _magicLinkServiceMock
            .Setup(x => x.ValidateMagicLinkAsync("token", "EmailVerification", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, (Guid?)Guid.NewGuid()));

        var result = await _sut.VerifyMagicLinkAsync("token", "EmailVerification");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyMagicLinkAsync_WhenInvalid_ReturnsFalse()
    {
        _magicLinkServiceMock
            .Setup(x => x.ValidateMagicLinkAsync("bad", "EmailVerification", It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, (Guid?)null));

        var result = await _sut.VerifyMagicLinkAsync("bad", "EmailVerification");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAuthResponseForUserAsync_WhenUserNotFound_ThrowsUnauthorizedAccessException()
    {
        var userId = Guid.NewGuid();
        _identityServiceMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((IdentityUserDto?)null);

        var act = () => _sut.GetAuthResponseForUserAsync(userId);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*User not found*");
    }

    [Fact]
    public async Task GetAuthResponseForUserAsync_WhenDomainProfileNotFound_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        _identityServiceMock
            .Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUserDto(userId, "u", "u@test.com", true, true, false, null, null, null, DateTime.UtcNow, null));
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var act = () => _sut.GetAuthResponseForUserAsync(userId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*User profile not found*");
    }

    [Fact]
    public async Task ResendMagicLinkAsync_WhenUserNotFound_ReturnsFalse()
    {
        _identityServiceMock
            .Setup(x => x.GetUserByEmailAsync("none@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityUserDto?)null);

        var result = await _sut.ResendMagicLinkAsync("none@test.com", "EmailVerification");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResendMagicLinkAsync_WhenUserFound_SendsEmailAndReturnsTrue()
    {
        var userId = Guid.NewGuid();
        var email = "u@test.com";
        _identityServiceMock
            .Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUserDto(userId, "u", email, false, true, false, null, null, null, DateTime.UtcNow, null));
        _magicLinkServiceMock
            .Setup(x => x.GenerateMagicLinkAsync(userId, "EmailVerification", email, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("new-token");
        _magicLinkServiceMock
            .Setup(x => x.BuildMagicLinkUrl("new-token", "EmailVerification", "https://test.local", It.IsAny<string?>(), null))
            .Returns("https://test.local/verify?token=new-token");

        var result = await _sut.ResendMagicLinkAsync(email, "EmailVerification");

        result.Should().BeTrue();
        _emailServiceMock.Verify(
            x => x.SendMagicLinkEmailAsync(email, It.IsAny<string>(), "u", "EmailVerification", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RequestPasswordResetAsync_WhenEmailNotFound_ReturnsTrueWithoutRevealing()
    {
        _identityServiceMock
            .Setup(x => x.GetUserByEmailAsync("unknown@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityUserDto?)null);

        var result = await _sut.RequestPasswordResetAsync("unknown@test.com");

        result.Should().BeTrue();
        _emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RequestPasswordlessLoginAsync_WhenUserNotFound_ReturnsTrueWithoutRevealing()
    {
        _identityServiceMock
            .Setup(x => x.GetUserByEmailAsync("unknown@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityUserDto?)null);

        var result = await _sut.RequestPasswordlessLoginAsync("unknown@test.com");

        result.Should().BeTrue();
        _emailServiceMock.Verify(x => x.SendMagicLinkEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RequestPasswordlessLoginAsync_WhenApplicationIdProvided_BuildsAppScopedMagicLink()
    {
        var applicationId = Guid.NewGuid();
        var clientId = "client-123";
        var userId = Guid.NewGuid();
        var email = "scoped@test.com";
        var app = AppApplication.Create(
            applicationId,
            Guid.NewGuid(),
            AppName.Create("Test App"),
            AppDescription.Create("Desc"),
            ApiKey.Create(clientId),
            ApiSecretHash.Create("hash"),
            _clockMock.Object.UtcNow);

        _identityServiceMock
            .Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUserDto(userId, "scoped-user", email, true, true, false, null, null, null, DateTime.UtcNow, null));
        _applicationAppServiceMock
            .Setup(x => x.GetByIdAsync(applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(app);
        _magicLinkServiceMock
            .Setup(x => x.GenerateMagicLinkAsync(userId, "PasswordlessLogin", email, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("magic-token");
        _magicLinkServiceMock
            .Setup(x => x.BuildMagicLinkUrl("magic-token", "PasswordlessLogin", "https://test.local", It.IsAny<string?>(), clientId))
            .Returns("https://test.local/magic-login#token=magic-token&purpose=PasswordlessLogin&clientId=" + clientId);

        var result = await _sut.RequestPasswordlessLoginAsync(email, applicationId);

        result.Should().BeTrue();
        _magicLinkServiceMock.Verify(
            x => x.BuildMagicLinkUrl("magic-token", "PasswordlessLogin", "https://test.local", It.IsAny<string?>(), clientId),
            Times.Once);
    }

    [Fact]
    public async Task VerifyMagicLinkAndAuthenticateAsync_WhenTokenInvalid_ThrowsUnauthorizedAccessException()
    {
        _magicLinkServiceMock
            .Setup(x => x.ValidateMagicLinkAsync("bad", "PasswordlessLogin", It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, (Guid?)null));

        var act = () => _sut.VerifyMagicLinkAndAuthenticateAsync("bad", "PasswordlessLogin");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid or expired magic link*");
    }

    [Fact]
    public async Task VerifyMagicLinkAndAuthenticateAsync_WhenValid_CallsGetAuthResponseForUserAsync()
    {
        var userId = Guid.NewGuid();
        var domainUser = CreateUser(userId, "user", "u@test.com", "F", "L");
        _magicLinkServiceMock
            .Setup(x => x.ValidateMagicLinkAsync("valid-token", "PasswordlessLogin", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, userId));
        _identityServiceMock
            .Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUserDto(userId, "u", "u@test.com", true, true, false, null, null, null, DateTime.UtcNow, null));
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(domainUser);
        _identityServiceMock.Setup(x => x.GetUserRolesAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<string> { "User" });
        _identityServiceMock.Setup(x => x.UpdateRefreshTokenAsync(userId, It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(userId, "u@test.com", "u", It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>())).Returns("access");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh");

        var result = await _sut.VerifyMagicLinkAndAuthenticateAsync("valid-token", "PasswordlessLogin");

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access");
    }

    [Fact]
    public async Task VerifyMagicLinkAndAuthenticateAsync_WhenApplicationIdProvided_UsesApplicationScopedPermissions()
    {
        var applicationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var domainUser = CreateUser(userId, "user", "u@test.com", "F", "L");

        _magicLinkServiceMock
            .Setup(x => x.ValidateMagicLinkAsync("valid-token", "PasswordlessLogin", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, userId));
        _identityServiceMock
            .Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUserDto(userId, "u", "u@test.com", true, true, false, null, null, null, DateTime.UtcNow, null));
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(domainUser);
        _identityServiceMock
            .Setup(x => x.GetUserRolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "User" });
        _userApplicationAppServiceMock
            .Setup(x => x.GetMembershipAsync(userId, applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserApplication.Create(Guid.NewGuid(), userId, applicationId, "Viewer"));
        _userPermissionServiceMock
            .Setup(x => x.GetPermissionCodesForUserAsync(userId, applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "articles.view" });
        _identityServiceMock
            .Setup(x => x.UpdateRefreshTokenAsync(userId, It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _tokenServiceMock
            .Setup(x => x.GenerateAccessToken(userId, "u@test.com", "u", It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), applicationId, null, null, null))
            .Returns("scoped-access");
        _tokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh");

        var result = await _sut.VerifyMagicLinkAndAuthenticateAsync("valid-token", "PasswordlessLogin", applicationId);

        result.AccessToken.Should().Be("scoped-access");
        _userPermissionServiceMock.Verify(
            x => x.GetPermissionCodesForUserAsync(userId, applicationId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ---------- Professional flows: Register failure & rollback ----------

    [Fact]
    public async Task RegisterUserAsync_WhenIdentityRegistrationFails_ThrowsWithJoinedErrors()
    {
        var email = "new@test.com";
        _identityServiceMock.Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync((IdentityUserDto?)null);
        _identityServiceMock
            .Setup(x => x.RegisterAsync(email, It.IsAny<string>(), "newuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, (Guid?)null, new[] { "Password too weak", "Username taken" }));

        var act = () => _sut.RegisterUserAsync(email, "x", "newuser", "First", "Last", new DateOnly(1990, 1, 1));

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .And.Message.Should().Contain("Password too weak")
            .And.Contain("Username taken");
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterUserAsync_WhenAddDomainUserThrows_RollbacksIdentityUser()
    {
        var userId = Guid.NewGuid();
        var email = "new@test.com";
        _identityServiceMock.Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync((IdentityUserDto?)null);
        _identityServiceMock
            .Setup(x => x.RegisterAsync(email, It.IsAny<string>(), "newuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, userId, Array.Empty<string>()));
        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB constraint"));
        _identityServiceMock
            .Setup(x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var act = () => _sut.RegisterUserAsync(email, "Pass123!", "newuser", "First", "Last", new DateOnly(1990, 1, 1));

        await act.Should().ThrowAsync<InvalidOperationException>();
        _identityServiceMock.Verify(x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_WhenSucceeds_PublishesUserRegisteredEvent()
    {
        var userId = Guid.NewGuid();
        var email = "evt@test.com";
        var username = "evtuser";
        _identityServiceMock.Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync((IdentityUserDto?)null);
        _identityServiceMock
            .Setup(x => x.RegisterAsync(email, It.IsAny<string>(), username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, userId, Array.Empty<string>()));
        _identityServiceMock.Setup(x => x.GetUserRolesAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<string> { "User" });
        _identityServiceMock.Setup(x => x.UpdateRefreshTokenAsync(userId, It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _identityServiceMock.Setup(x => x.UpdateUserProfileIdAsync(userId, It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var domainUser = CreateUser(userId, username, email, "F", "L");
        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(domainUser);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(domainUser);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>())).Returns("access");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh");

        DomainEventNotification<UserRegisteredEvent>? captured = null;
        _mediatorMock.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Callback<INotification, CancellationToken>((n, _) =>
            {
                if (n is DomainEventNotification<UserRegisteredEvent> notif)
                    captured = notif;
            });

        await _sut.RegisterUserAsync(email, "Pass123!", username, "F", "L", new DateOnly(1990, 1, 1));

        captured.Should().NotBeNull();
        captured!.DomainEvent.UserId.Should().Be(userId);
        captured.DomainEvent.Email.Should().Be(email);
        captured.DomainEvent.Username.Should().Be(username);
    }

    // ---------- Parameterized & edge cases ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RefreshTokenAsync_WhenTokenNullOrWhitespace_ThrowsUnauthorizedAccessException(string? token)
    {
        var act = () => _sut.RefreshTokenAsync(token!);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid refresh token*");
    }

    [Theory]
    [InlineData("EmailVerification")]
    [InlineData("PasswordReset")]
    [InlineData("PasswordlessLogin")]
    public async Task ResendMagicLinkAsync_WithDifferentPurposes_CallsGenerateMagicLinkWithSamePurpose(string purpose)
    {
        var userId = Guid.NewGuid();
        var email = "purpose@test.com";
        _identityServiceMock
            .Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUserDto(userId, "u", email, false, true, false, null, null, null, DateTime.UtcNow, null));
        _magicLinkServiceMock
            .Setup(x => x.GenerateMagicLinkAsync(userId, purpose, email, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("token");
        _magicLinkServiceMock.Setup(x => x.BuildMagicLinkUrl("token", purpose, It.IsAny<string>(), It.IsAny<string?>(), null)).Returns("https://link");

        await _sut.ResendMagicLinkAsync(email, purpose);

        _magicLinkServiceMock.Verify(
            x => x.GenerateMagicLinkAsync(userId, purpose, email, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAuthResponseForUserAsync_WhenUserHasMultipleRoles_PassesRolesToTokenService()
    {
        var userId = Guid.NewGuid();
        var domainUser = CreateUser(userId, "artist", "a@test.com", "A", "B");
        var roles = new List<string> { "User", "Artist" };
        _identityServiceMock
            .Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUserDto(userId, "artist", "a@test.com", true, true, false, null, null, null, DateTime.UtcNow, null));
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(domainUser);
        _identityServiceMock.Setup(x => x.GetUserRolesAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(roles);
        _identityServiceMock.Setup(x => x.UpdateRefreshTokenAsync(userId, It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(userId, "a@test.com", "artist", It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>())).Returns("access");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh");

        var result = await _sut.GetAuthResponseForUserAsync(userId);

        result.User.Roles.Should().BeEquivalentTo(roles);
        _tokenServiceMock.Verify(
            x => x.GenerateAccessToken(userId, "a@test.com", "artist", It.Is<IEnumerable<string>>(r => r.SequenceEqual(roles)), It.IsAny<IEnumerable<string>>()),
            Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenDomainUserIsNull_StillReturnsTrueAndDoesNotSendWelcomeEmail()
    {
        var userId = Guid.NewGuid();
        _identityServiceMock
            .Setup(x => x.VerifyEmailAsync(userId, "token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await _sut.VerifyEmailAsync(userId, "token");

        result.Should().BeTrue();
        _emailServiceMock.Verify(x => x.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- Case đặc biệt: Empty / Null / Boundary ----------

    [Fact]
    public async Task ResendMagicLinkAsync_WhenEmailIsEmpty_IdentityReturnsNull_ReturnsFalse()
    {
        _identityServiceMock.Setup(x => x.GetUserByEmailAsync("", It.IsAny<CancellationToken>())).ReturnsAsync((IdentityUserDto?)null);

        var result = await _sut.ResendMagicLinkAsync("", "EmailVerification");

        result.Should().BeFalse();
        _emailServiceMock.Verify(x => x.SendMagicLinkEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VerifyMagicLinkAndAuthenticateAsync_WhenValidateReturnsValidButUserIdNull_ThrowsUnauthorized()
    {
        _magicLinkServiceMock
            .Setup(x => x.ValidateMagicLinkAsync("token", "EmailVerification", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, (Guid?)null));

        var act = () => _sut.VerifyMagicLinkAndAuthenticateAsync("token", "EmailVerification");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid or expired magic link*");
    }

    [Fact]
    public async Task GetAuthResponseForUserAsync_WhenUserIdIsEmptyGuid_CallsIdentityWithThatGuid()
    {
        var emptyGuid = Guid.Empty;
        _identityServiceMock.Setup(x => x.GetUserByIdAsync(emptyGuid, It.IsAny<CancellationToken>())).ReturnsAsync((IdentityUserDto?)null);

        var act = () => _sut.GetAuthResponseForUserAsync(emptyGuid);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*User not found*");
        _identityServiceMock.Verify(x => x.GetUserByIdAsync(emptyGuid, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestPasswordResetAsync_WhenEmailIsWhitespace_DoesNotRevealWhetherUserExists()
    {
        _identityServiceMock.Setup(x => x.GetUserByEmailAsync("   ", It.IsAny<CancellationToken>())).ReturnsAsync((IdentityUserDto?)null);

        var result = await _sut.RequestPasswordResetAsync("   ");

        result.Should().BeTrue();
        _emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginUserAsync_WhenIdentitySucceedsButUserDtoNull_ThrowsUnauthorized()
    {
        _identityServiceMock
            .Setup(x => x.LoginAsync("x@test.com", "Pass1!", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, (IdentityUserDto?)null, Array.Empty<string>()));

        var act = () => _sut.LoginUserAsync("x@test.com", "Pass1!");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ResendMagicLinkAsync_WhenIdentityThrows_PropagatesException()
    {
        _identityServiceMock
            .Setup(x => x.GetUserByEmailAsync("fail@test.com", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB connection failed"));

        var act = () => _sut.ResendMagicLinkAsync("fail@test.com", "EmailVerification");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*DB connection failed*");
    }

    private static User CreateUser(
        Guid id,
        string username,
        string email,
        string firstName,
        string lastName,
        DateOnly? dateOfBirth = null)
        => User.Create(
            id,
            Username.Create(username),
            Email.Create(email),
            PersonName.Create(firstName, lastName),
            dateOfBirth ?? new DateOnly(1990, 1, 1),
            new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc));
}

