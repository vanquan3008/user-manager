using System.Text;
using UserService.Application.Common;
using UserService.Application.Services.Security;

namespace UserService.Tests.Security;

public class TotpServiceTests
{
    private readonly TotpService _sut = new();
    // RFC 6238 Appendix B SHA-1 secret.
    private static readonly byte[] RfcSecret = Encoding.ASCII.GetBytes("12345678901234567890");

    [Theory]
    [InlineData(59L, "287082")]
    [InlineData(1111111109L, "081804")]
    [InlineData(1111111111L, "050471")]
    [InlineData(1234567890L, "005924")]
    [InlineData(2000000000L, "279037")]
    public void ComputeCode_MatchesRfc6238TestVectors(long unixSeconds, string expected)
        => _sut.ComputeCode(RfcSecret, unixSeconds / 30).Should().Be(expected);

    [Fact]
    public void TryValidate_AcceptsCurrentAndAdjacentStep_RejectsOlder()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1234567890).UtcDateTime;
        var step = TotpService.GetTimeStep(now);
        var secret = Base32.Encode(RfcSecret);

        _sut.TryValidate(secret, _sut.ComputeCode(RfcSecret, step), now, null, out var matched).Should().BeTrue();
        matched.Should().Be(step);
        _sut.TryValidate(secret, _sut.ComputeCode(RfcSecret, step - 1), now, null, out _).Should().BeTrue("one step of clock drift is allowed");
        _sut.TryValidate(secret, _sut.ComputeCode(RfcSecret, step - 2), now, null, out _).Should().BeFalse();
    }

    [Fact]
    public void TryValidate_RejectsReplayOfAlreadyUsedTimeStep()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(2000000000).UtcDateTime;
        var step = TotpService.GetTimeStep(now);
        var code = _sut.ComputeCode(RfcSecret, step);

        _sut.TryValidate(Base32.Encode(RfcSecret), code, now, lastUsedTimeStep: step, out _).Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("abcdef")]
    [InlineData("1234567")]
    public void TryValidate_RejectsMalformedCodes(string code)
        => _sut.TryValidate(Base32.Encode(RfcSecret), code, DateTime.UtcNow, null, out _).Should().BeFalse();

    [Fact]
    public void Base32_RoundTrips()
    {
        var data = System.Security.Cryptography.RandomNumberGenerator.GetBytes(20);
        Base32.Decode(Base32.Encode(data)).Should().Equal(data);
        Base32.Decode("gezd gnbv").Should().Equal(Encoding.ASCII.GetBytes("12345"));
    }

    [Fact]
    public void AuthenticatorUri_IsOtpauthWithIssuerAndSecret()
    {
        var uri = _sut.BuildAuthenticatorUri("Acme SSO", "jane@acme.test", "JBSWY3DPEHPK3PXP");
        uri.Should().StartWith("otpauth://totp/Acme%20SSO%3Ajane%40acme.test?");
        uri.Should().Contain("secret=JBSWY3DPEHPK3PXP").And.Contain("issuer=Acme%20SSO").And.Contain("period=30");
    }
}
