using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Configuration;
using UserService.Application.DTOs.Auth;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;
using UserService.Infrastructure.Services;

namespace UserService.Tests.Services;

public class TokenServiceTests
{
    private readonly Mock<IClaimsFactory> _claimsFactoryMock;
    private readonly AppSettings _appSettings;
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        _claimsFactoryMock = new Mock<IClaimsFactory>();
        _appSettings = new AppSettings
        {
            Jwt = new JwtConfiguration
            {
                SecretKey = "test-secret-key-must-be-at-least-32-characters-long",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                AccessTokenExpirationMinutes = 60
            }
        };
        _sut = new TokenService(_claimsFactoryMock.Object, _appSettings);
    }

    [Fact]
    public void GenerateAccessToken_WhenSecretKeyMissing_ThrowsInvalidOperationException()
    {
        var svc = new TokenService(_claimsFactoryMock.Object, new AppSettings { Jwt = new JwtConfiguration { SecretKey = "" } });
        _claimsFactoryMock.Setup(x => x.CreateAccessTokenClaims(It.IsAny<AccessTokenRequest>()))
            .Returns(new List<Claim>());

        var act = () => svc.GenerateAccessToken(new AccessTokenRequest(Guid.NewGuid(), "u@test.com", "u", Array.Empty<string>()));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SecretKey*");
    }

    [Fact]
    public void GenerateAccessToken_WithValidSettings_ReturnsNonEmptyJwt()
    {
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, "u@test.com")
        };
        _claimsFactoryMock
            .Setup(x => x.CreateAccessTokenClaims(It.Is<AccessTokenRequest>(r => r.UserId == userId)))
            .Returns(claims);

        var token = _sut.GenerateAccessToken(new AccessTokenRequest(userId, "u@test.com", "u", new[] { "User" }));

        token.Should().NotBeNullOrWhiteSpace();
        token.Split('.').Length.Should().Be(3); // JWT has 3 parts
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyBase64String()
    {
        var token = _sut.GenerateRefreshToken();

        token.Should().NotBeNullOrWhiteSpace();
        Convert.FromBase64String(token).Length.Should().Be(64);
    }

    [Fact]
    public void GenerateRefreshToken_EachCallReturnsDifferentValue()
    {
        var t1 = _sut.GenerateRefreshToken();
        var t2 = _sut.GenerateRefreshToken();

        t1.Should().NotBe(t2);
    }

    [Fact]
    public void ValidateToken_WhenTokenGeneratedBySameService_ReturnsTrue()
    {
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, "u@test.com")
        };
        _claimsFactoryMock
            .Setup(x => x.CreateAccessTokenClaims(It.Is<AccessTokenRequest>(r => r.UserId == userId)))
            .Returns(claims);

        var token = _sut.GenerateAccessToken(new AccessTokenRequest(userId, "u@test.com", null, Array.Empty<string>()));
        var isValid = _sut.ValidateToken(token);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_WhenTokenInvalid_ReturnsFalse()
    {
        var isValid = _sut.ValidateToken("invalid.jwt.token");

        isValid.Should().BeFalse();
    }

    [Fact]
    public void GetUserIdFromToken_WhenTokenValid_ReturnsUserId()
    {
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, "u@test.com")
        };
        _claimsFactoryMock
            .Setup(x => x.CreateAccessTokenClaims(It.Is<AccessTokenRequest>(r => r.UserId == userId)))
            .Returns(claims);

        var token = _sut.GenerateAccessToken(new AccessTokenRequest(userId, "u@test.com", null, Array.Empty<string>()));
        var extractedId = _sut.GetUserIdFromToken(token);

        extractedId.Should().Be(userId);
    }

    [Fact]
    public void GetUserIdFromToken_WhenTokenInvalid_ReturnsNull()
    {
        var result = _sut.GetUserIdFromToken("not-a-valid-jwt");

        result.Should().BeNull();
    }

    // ---------- Professional flows: parameterized & edge cases ----------

    [Theory]
    [InlineData(null, "User")]
    [InlineData("Display Name", "User", "Artist")]
    [InlineData("", "Admin")]
    public void GenerateAccessToken_WithVariousDisplayNameAndRoles_ProducesValidTokenAndExtractableUserId(string? displayName, params string[] roles)
    {
        var userId = Guid.NewGuid();
        var email = "u@test.com";
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email)
        };
        claims.AddRange(roles.Select(r => new Claim(System.Security.Claims.ClaimTypes.Role, r)));
        _claimsFactoryMock
            .Setup(x => x.CreateAccessTokenClaims(It.Is<AccessTokenRequest>(r => r.UserId == userId && r.Email == email && r.DisplayName == displayName)))
            .Returns(claims);

        var token = _sut.GenerateAccessToken(new AccessTokenRequest(userId, email, displayName, roles));

        token.Should().NotBeNullOrWhiteSpace();
        _sut.ValidateToken(token).Should().BeTrue();
        _sut.GetUserIdFromToken(token).Should().Be(userId);
    }

    [Fact]
    public void GenerateAccessToken_WithEmptyRoles_UsesClaimsFactoryWithEmptyEnumerable()
    {
        var userId = Guid.NewGuid();
        _claimsFactoryMock
            .Setup(x => x.CreateAccessTokenClaims(It.Is<AccessTokenRequest>(r => r.UserId == userId && r.Email == "e@test.com" && r.DisplayName == null && !r.Roles.Any())))
            .Returns(new List<Claim> { new(JwtRegisteredClaimNames.Sub, userId.ToString()) });

        _sut.GenerateAccessToken(new AccessTokenRequest(userId, "e@test.com", null, Array.Empty<string>()));

        _claimsFactoryMock.Verify(
            x => x.CreateAccessTokenClaims(It.Is<AccessTokenRequest>(r => r.UserId == userId && r.Email == "e@test.com" && r.DisplayName == null && !r.Roles.Any())),
            Times.Once);
    }

    [Fact]
    public void GenerateRefreshToken_ProducesBase64DecodableValue()
    {
        var token = _sut.GenerateRefreshToken();

        var act = () => Convert.FromBase64String(token);
        act.Should().NotThrow();
        Convert.FromBase64String(token).Length.Should().Be(64);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("a.b.c")]
    [InlineData("not-three-parts")]
    public void ValidateToken_WithInvalidFormat_ReturnsFalse(string invalidToken)
    {
        _sut.ValidateToken(invalidToken).Should().BeFalse();
    }

    // ---------- Case đặc biệt: Null / Empty / Boundary ----------

    [Fact]
    public void GetUserIdFromToken_WhenTokenIsNull_ReturnsNull()
    {
        var result = _sut.GetUserIdFromToken(null!);

        result.Should().BeNull();
    }

    [Fact]
    public void GetUserIdFromToken_WhenTokenIsEmptyString_ReturnsNull()
    {
        var result = _sut.GetUserIdFromToken("");

        result.Should().BeNull();
    }

    [Fact]
    public void GenerateAccessToken_WhenSecretKeyExactly32Characters_ProducesValidToken()
    {
        var key32 = "12345678901234567890123456789012";
        var svc = new TokenService(_claimsFactoryMock.Object, new AppSettings
        {
            Jwt = new JwtConfiguration
            {
                SecretKey = key32,
                Issuer = "I",
                Audience = "A",
                AccessTokenExpirationMinutes = 60
            }
        });
        var userId = Guid.NewGuid();
        _claimsFactoryMock
            .Setup(x => x.CreateAccessTokenClaims(It.Is<AccessTokenRequest>(r => r.UserId == userId && r.Email == "e@t.com")))
            .Returns(new List<Claim> { new(JwtRegisteredClaimNames.Sub, userId.ToString()) });

        var token = svc.GenerateAccessToken(new AccessTokenRequest(userId, "e@t.com", null, Array.Empty<string>()));

        token.Should().NotBeNullOrWhiteSpace();
        svc.ValidateToken(token).Should().BeTrue();
        svc.GetUserIdFromToken(token).Should().Be(userId);
    }

    [Fact]
    public void GenerateAccessToken_WhenEmailIsEmpty_StillProducesTokenWithSubClaim()
    {
        var userId = Guid.NewGuid();
        _claimsFactoryMock
            .Setup(x => x.CreateAccessTokenClaims(It.Is<AccessTokenRequest>(r => r.UserId == userId && r.Email == "")))
            .Returns(new List<Claim> { new(JwtRegisteredClaimNames.Sub, userId.ToString()) });

        var token = _sut.GenerateAccessToken(new AccessTokenRequest(userId, "", null, Array.Empty<string>()));

        token.Should().NotBeNullOrWhiteSpace();
        _sut.GetUserIdFromToken(token).Should().Be(userId);
    }

    [Fact]
    public void GenerateRefreshToken_WhenCalledManyTimes_AllTokensUnique()
    {
        var tokens = new HashSet<string>();
        for (var i = 0; i < 50; i++)
            tokens.Add(_sut.GenerateRefreshToken());

        tokens.Should().HaveCount(50);
    }
}
