using Microsoft.Extensions.Caching.Memory;
using UserService.Application.Common;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services;
using UserService.Application.Interfaces.Services.Security;
using UserService.Application.Services.Security;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;
using UserService.Tests.TestDoubles;

namespace UserService.Tests.Security;

public class SessionServiceTests
{
    private readonly List<UserSession> _store = new();
    private readonly Mock<IUserSessionRepository> _sessions = new();
    private readonly Mock<IOAuthRefreshTokenRepository> _oauthTokens = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IAuditLogger> _audit = new();
    private readonly Mock<IBackchannelLogoutService> _backchannel = new();
    private readonly FakeClock _clock = new();
    private readonly SessionService _sut;

    public SessionServiceTests()
    {
        _sessions.Setup(x => x.AddAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()))
            .Callback<UserSession, CancellationToken>((s, _) => _store.Add(s)).Returns(Task.CompletedTask);
        _sessions.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => _store.FirstOrDefault(s => s.Id == id));
        _sessions.Setup(x => x.GetByRefreshTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string h, CancellationToken _) => _store.FirstOrDefault(s => s.RefreshTokenHash == h));
        _sessions.Setup(x => x.GetByPreviousRefreshTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string h, CancellationToken _) => _store.FirstOrDefault(s => s.PreviousRefreshTokenHash == h));
        _sessions.Setup(x => x.GetActiveForUserAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid u, DateTime now, CancellationToken _) => _store.Where(s => s.UserId == u && s.IsActive(now)).ToList());
        _oauthTokens.Setup(x => x.RevokeAllForSessionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { Guid.NewGuid() });

        var settings = new AppSettings
        {
            Jwt = new JwtConfiguration { RefreshTokenExpirationDays = 7 },
            Security = new SecurityConfiguration { RefreshTokenReuseGraceSeconds = 30, SessionAbsoluteLifetimeDays = 30, SessionValidationCacheSeconds = 15 }
        };
        _sut = new SessionService(_sessions.Object, _oauthTokens.Object, _uow.Object, _clock, settings,
            new MemoryCache(new MemoryCacheOptions()), new FakeRequestContext(), _audit.Object, _backchannel.Object);
    }

    [Fact]
    public async Task Start_StoresOnlyTheHashOfTheRefreshToken()
    {
        var issued = await _sut.StartAsync(Guid.NewGuid(), UserSessionKinds.Console, AuthenticationMethods.Password);

        issued.Session.RefreshTokenHash.Should().Be(SecureTokens.Hash(issued.RefreshToken));
        issued.Session.RefreshTokenHash.Should().NotContain(issued.RefreshToken);
        issued.Session.IpAddress.Should().Be("203.0.113.10");
    }

    [Fact]
    public async Task Rotate_IssuesNewToken_AndInvalidatesTheOldOne()
    {
        var first = await _sut.StartAsync(Guid.NewGuid(), UserSessionKinds.Console, AuthenticationMethods.Password);
        var second = await _sut.RotateAsync(first.RefreshToken);

        second.RefreshToken.Should().NotBe(first.RefreshToken);
        second.Session.Id.Should().Be(first.Session.Id, "rotation keeps the same session (sid)");
        (await _sut.IsActiveAsync(first.Session.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task ReplayWithinGracePeriod_IsRejectedWithoutRevokingTheSession()
    {
        var first = await _sut.StartAsync(Guid.NewGuid(), UserSessionKinds.Console, AuthenticationMethods.Password);
        await _sut.RotateAsync(first.RefreshToken);
        _clock.Advance(TimeSpan.FromSeconds(5));

        var act = () => _sut.RotateAsync(first.RefreshToken);

        (await act.Should().ThrowAsync<AuthenticationFlowException>()).Which.Code.Should().Be(AuthErrorCodes.RefreshTokenSuperseded);
        first.Session.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task ReplayAfterGracePeriod_RevokesSession_Grants_AndNotifiesRelyingParties()
    {
        var first = await _sut.StartAsync(Guid.NewGuid(), UserSessionKinds.HostedSso, AuthenticationMethods.Password);
        var second = await _sut.RotateAsync(first.RefreshToken);
        _clock.Advance(TimeSpan.FromMinutes(5));

        var act = () => _sut.RotateAsync(first.RefreshToken);

        (await act.Should().ThrowAsync<AuthenticationFlowException>()).Which.Code.Should().Be(AuthErrorCodes.RefreshTokenReused);
        first.Session.RevokedReason.Should().Be(SessionRevocationReasons.RefreshTokenReuse);
        _oauthTokens.Verify(x => x.RevokeAllForSessionAsync(first.Session.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _backchannel.Verify(x => x.NotifyApplicationsAsync(first.Session.UserId, first.Session.Id, It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
        _audit.Verify(x => x.LogAsync(AuditEventTypes.RefreshTokenReuseDetected, false, It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()), Times.Once);

        // The legitimate holder's newer token is dead too.
        var legit = () => _sut.RotateAsync(second.RefreshToken);
        (await legit.Should().ThrowAsync<AuthenticationFlowException>()).Which.Code.Should().Be(AuthErrorCodes.SessionExpired);
        (await _sut.IsActiveAsync(first.Session.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task UnknownToken_IsInvalid()
    {
        var act = () => _sut.RotateAsync("does-not-exist");
        (await act.Should().ThrowAsync<AuthenticationFlowException>()).Which.Code.Should().Be(AuthErrorCodes.InvalidRefreshToken);
    }

    [Fact]
    public async Task ConcurrentRotation_LosingRequestGetsSuperseded()
    {
        var first = await _sut.StartAsync(Guid.NewGuid(), UserSessionKinds.Console, AuthenticationMethods.Password);
        _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new ConcurrencyConflictException("conflict"));

        var act = () => _sut.RotateAsync(first.RefreshToken);
        (await act.Should().ThrowAsync<AuthenticationFlowException>()).Which.Code.Should().Be(AuthErrorCodes.RefreshTokenSuperseded);
    }

    [Fact]
    public async Task RevokeAll_KeepsTheExcludedSession()
    {
        var userId = Guid.NewGuid();
        var keep = await _sut.StartAsync(userId, UserSessionKinds.Console, AuthenticationMethods.Password);
        var a = await _sut.StartAsync(userId, UserSessionKinds.Console, AuthenticationMethods.Password);
        var b = await _sut.StartAsync(userId, UserSessionKinds.HostedSso, AuthenticationMethods.Password);

        var count = await _sut.RevokeAllAsync(userId, SessionRevocationReasons.PasswordChanged, keep.Session.Id);

        count.Should().Be(2);
        keep.Session.IsRevoked.Should().BeFalse();
        a.Session.IsRevoked.Should().BeTrue();
        b.Session.IsRevoked.Should().BeTrue();
    }
}
