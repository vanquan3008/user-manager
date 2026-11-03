using UserService.Domain.Entities;

namespace UserService.Tests.Security;

public class UserSessionTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static UserSession NewSession() => UserSession.Start(
        Guid.NewGuid(), "hash-1", UserSessionKinds.Console, AuthenticationMethods.Password,
        Now, TimeSpan.FromDays(7), TimeSpan.FromDays(30), "1.2.3.4", "ua");

    [Fact]
    public void Rotate_KeepsPreviousHash_AndSlidesIdleExpiry_ButNotPastAbsoluteLifetime()
    {
        var s = NewSession();
        var version = s.Version;

        s.Rotate("hash-2", Now.AddDays(6), TimeSpan.FromDays(7), null, null);
        s.RefreshTokenHash.Should().Be("hash-2");
        s.PreviousRefreshTokenHash.Should().Be("hash-1");
        s.ExpiresAt.Should().Be(Now.AddDays(13));
        s.Version.Should().NotBe(version);

        s.Rotate("hash-3", Now.AddDays(12), TimeSpan.FromDays(7), null, null);
        s.Rotate("hash-4", Now.AddDays(18), TimeSpan.FromDays(7), null, null);
        s.Rotate("hash-5", Now.AddDays(24), TimeSpan.FromDays(7), null, null);
        s.ExpiresAt.Should().Be(Now.AddDays(30), "the absolute lifetime caps sliding renewal");
    }

    [Fact]
    public void Revoked_Or_Expired_Session_IsInactive_AndCannotRotate()
    {
        var s = NewSession();
        s.IsActive(Now.AddDays(8)).Should().BeFalse();

        var r = NewSession();
        r.Revoke(SessionRevocationReasons.Logout, Now).Should().BeTrue();
        r.Revoke(SessionRevocationReasons.Logout, Now).Should().BeFalse("revocation is idempotent");
        r.IsActive(Now).Should().BeFalse();
        r.Invoking(x => x.Rotate("h", Now, TimeSpan.FromDays(1), null, null)).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GracePeriod_OnlyAppliesShortlyAfterRotation()
    {
        var s = NewSession();
        s.IsWithinRotationGracePeriod(Now, TimeSpan.FromSeconds(30)).Should().BeFalse("never rotated");
        s.Rotate("hash-2", Now, TimeSpan.FromDays(7), null, null);
        s.IsWithinRotationGracePeriod(Now.AddSeconds(10), TimeSpan.FromSeconds(30)).Should().BeTrue();
        s.IsWithinRotationGracePeriod(Now.AddSeconds(31), TimeSpan.FromSeconds(30)).Should().BeFalse();
    }
}
