using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;
using UserService.Domain.Constants;
using UserService.Tests.TestDoubles;

namespace UserService.Tests.Services;

public class TokenServiceTests
{
    private readonly AppSettings _settings = new()
    {
        Jwt = new JwtConfiguration { Issuer = "https://sso.test", Audience = "sso-api", AccessTokenExpirationMinutes = 15 }
    };
    private readonly TestSigningKeyProvider _keys = new();
    private readonly FakeClock _clock = new() { UtcNow = DateTime.UtcNow };
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        _sut = new TokenService(new ClaimsFactory(), _settings, _keys, _clock);
    }

    private static AccessTokenRequest Request(Guid userId, Guid? sid = null) =>
        new(userId, "u@test.com", "u", new[] { "Admin" }, new[] { "USER.READ" }, SessionId: sid, ClientId: "client-1");

    [Fact]
    public void AccessToken_IsRs256_WithKeyIdAndAtJwtType()
    {
        var token = _sut.GenerateAccessToken(Request(Guid.NewGuid()));
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Header.Alg.Should().Be("RS256");
        jwt.Header.Kid.Should().Be(_keys.Active.KeyId);
        jwt.Header.Typ.Should().Be("at+jwt");
        jwt.Issuer.Should().Be("https://sso.test");
        jwt.Audiences.Should().ContainSingle("sso-api");
    }

    [Fact]
    public void AccessToken_CarriesSessionIdClientAndPermissions()
    {
        var userId = Guid.NewGuid();
        var sid = Guid.NewGuid();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(_sut.GenerateAccessToken(Request(userId, sid)));

        jwt.Subject.Should().Be(userId.ToString());
        jwt.Claims.Should().Contain(c => c.Type == JwtClaimNames.SessionId && c.Value == sid.ToString());
        jwt.Claims.Should().Contain(c => c.Type == JwtClaimNames.ClientId && c.Value == "client-1");
        jwt.Claims.Should().Contain(c => c.Type == JwtClaimNames.Permissions && c.Value == "USER.READ");
        jwt.ValidTo.Should().BeCloseTo(_clock.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ValidateToken_AcceptsOwnTokens_AndRejectsGarbage()
    {
        var userId = Guid.NewGuid();
        var token = _sut.GenerateAccessToken(Request(userId));

        _sut.ValidateToken(token).Should().BeTrue();
        _sut.GetUserIdFromToken(token).Should().Be(userId);
        _sut.ValidateToken("not.a.jwt").Should().BeFalse();
        _sut.ValidateToken("").Should().BeFalse();
        _sut.GetUserIdFromToken("garbage").Should().BeNull();
    }

    [Fact]
    public void ValidateToken_RejectsTokenSignedByUnknownKey()
    {
        var foreign = new TokenService(new ClaimsFactory(), _settings, new TestSigningKeyProvider(), _clock);
        _sut.ValidateToken(foreign.GenerateAccessToken(Request(Guid.NewGuid()))).Should().BeFalse();
    }

    [Fact]
    public void KeyRotation_KeepsOldTokensValidUntilKeyIsRemoved()
    {
        var oldKey = _keys.Active;
        var tokenFromOldKey = _sut.GenerateAccessToken(Request(Guid.NewGuid()));

        _keys.AddKey(); // rotate: new tokens use the new key, old key still published
        var tokenFromNewKey = _sut.GenerateAccessToken(Request(Guid.NewGuid()));

        new JwtSecurityTokenHandler().ReadJwtToken(tokenFromNewKey).Header.Kid.Should().NotBe(oldKey.KeyId);
        _sut.ValidateToken(tokenFromOldKey).Should().BeTrue();
        _sut.ValidateToken(tokenFromNewKey).Should().BeTrue();

        _keys.RemoveKey(oldKey); // retention window over
        _sut.ValidateToken(tokenFromOldKey).Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_RejectsWrongAudience()
    {
        var other = new TokenService(new ClaimsFactory(),
            new AppSettings { Jwt = new JwtConfiguration { Issuer = "https://sso.test", Audience = "someone-else", AccessTokenExpirationMinutes = 15 } },
            _keys, _clock);
        _sut.ValidateToken(other.GenerateAccessToken(Request(Guid.NewGuid()))).Should().BeFalse();
    }

    [Fact]
    public void IdToken_HasClientAudienceNonceSidAndScopedClaims()
    {
        var userId = Guid.NewGuid();
        var sid = Guid.NewGuid();
        var token = _sut.GenerateIdToken(new IdTokenRequest(
            userId, "client-1", "nonce-abc", sid, _clock.UtcNow.AddMinutes(-3),
            new[] { "openid", "email" }, "u@test.com", true, "User", "user", "mfa"));
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Audiences.Should().ContainSingle("client-1");
        jwt.Claims.Should().Contain(c => c.Type == "nonce" && c.Value == "nonce-abc");
        jwt.Claims.Should().Contain(c => c.Type == "sid" && c.Value == sid.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == "u@test.com");
        jwt.Claims.Should().Contain(c => c.Type == "amr" && c.Value == "mfa");
        jwt.Claims.Should().Contain(c => c.Type == "auth_time");
        jwt.Claims.Should().NotContain(c => c.Type == "name", "profile scope was not granted");
    }

    [Fact]
    public void LogoutToken_FollowsBackChannelLogoutSpec()
    {
        var sid = Guid.NewGuid().ToString();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(_sut.GenerateLogoutToken(Guid.NewGuid(), sid, "client-1"));

        jwt.Header.Typ.Should().Be("logout+jwt");
        jwt.Audiences.Should().ContainSingle("client-1");
        jwt.Claims.Should().Contain(c => c.Type == "sid" && c.Value == sid);
        jwt.Claims.Should().Contain(c => c.Type == "jti");
        jwt.Claims.Should().Contain(c => c.Type == "events" && c.Value.Contains("http://schemas.openid.net/event/backchannel-logout"));
        jwt.Claims.Should().NotContain(c => c.Type == "nonce");
    }

    [Fact]
    public void RefreshTokens_AreUrlSafeAndUnique()
    {
        var tokens = Enumerable.Range(0, 200).Select(_ => _sut.GenerateRefreshToken()).ToList();
        tokens.Should().OnlyHaveUniqueItems();
        tokens.Should().OnlyContain(t => t.Length >= 43 && t.All(c => char.IsAsciiLetterOrDigit(c) || c == '-' || c == '_'));
    }
}
