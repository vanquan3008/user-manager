using System.Buffers.Binary;
using System.Security.Cryptography;
using UserService.Application.Common;
using UserService.Application.Interfaces.Services.Security;

namespace UserService.Application.Services.Security;

/// <summary>RFC 6238 TOTP (HMAC-SHA1, 6 digits, 30-second step) compatible with standard authenticator apps.</summary>
public sealed class TotpService : ITotpService
{
    private const int StepSeconds = 30;
    private const int Digits = 6;
    private const int AllowedDriftSteps = 1;

    public bool TryValidate(string base32Secret, string code, DateTime utcNow, long? lastUsedTimeStep, out long matchedTimeStep)
    {
        matchedTimeStep = 0;
        var normalized = (code ?? string.Empty).Replace(" ", string.Empty).Replace("-", string.Empty);
        if (normalized.Length != Digits || !normalized.All(char.IsAsciiDigit))
            return false;

        byte[] secret;
        try { secret = Base32.Decode(base32Secret); }
        catch (FormatException) { return false; }
        if (secret.Length == 0)
            return false;

        var currentStep = GetTimeStep(utcNow);
        for (var offset = -AllowedDriftSteps; offset <= AllowedDriftSteps; offset++)
        {
            var step = currentStep + offset;
            if (lastUsedTimeStep.HasValue && step <= lastUsedTimeStep.Value)
                continue;

            if (SecureTokens.FixedTimeEquals(ComputeCode(secret, step), normalized))
            {
                matchedTimeStep = step;
                return true;
            }
        }

        return false;
    }

    public string ComputeCode(byte[] secret, long timeStep)
    {
        Span<byte> counter = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(counter, timeStep);
        var hash = HMACSHA1.HashData(secret, counter);
        var offset = hash[^1] & 0x0F;
        var binary = ((hash[offset] & 0x7F) << 24)
                     | (hash[offset + 1] << 16)
                     | (hash[offset + 2] << 8)
                     | hash[offset + 3];
        var otp = binary % (int)Math.Pow(10, Digits);
        return otp.ToString(new string('0', Digits));
    }

    public string BuildAuthenticatorUri(string issuer, string accountName, string base32Secret)
    {
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var label = Uri.EscapeDataString($"{issuer}:{accountName}");
        return $"otpauth://totp/{label}?secret={base32Secret}&issuer={encodedIssuer}&algorithm=SHA1&digits={Digits}&period={StepSeconds}";
    }

    public static long GetTimeStep(DateTime utcNow)
        => new DateTimeOffset(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc)).ToUnixTimeSeconds() / StepSeconds;
}
