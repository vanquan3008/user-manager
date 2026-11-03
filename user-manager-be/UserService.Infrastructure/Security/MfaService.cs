using Microsoft.AspNetCore.Identity;
using UserService.Application.Common;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Identity;

namespace UserService.Infrastructure.Security;

/// <summary>
/// TOTP-based multi-factor authentication on top of ASP.NET Core Identity.
/// The authenticator secret uses Identity's token store; recovery codes are stored as SHA-256 hashes
/// (Identity's default store keeps them in plaintext, so it is not used).
/// </summary>
public sealed class MfaService : IMfaService
{
    private const string RecoveryProvider = "[UserServiceMfa]";
    private const string RecoveryTokenName = "RecoveryCodeHashes";

    private readonly UserManager<AppUser> _users;
    private readonly ITotpService _totp;
    private readonly IClock _clock;
    private readonly AppSettings _settings;

    public MfaService(UserManager<AppUser> users, ITotpService totp, IClock clock, AppSettings settings)
    {
        _users = users;
        _totp = totp;
        _clock = clock;
        _settings = settings;
    }

    public async Task<MfaStatus> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await RequireUserAsync(userId);
        var hashes = await GetRecoveryHashesAsync(user);
        return new MfaStatus(user.TwoFactorEnabled, user.MfaEnabledAt, user.TwoFactorEnabled ? hashes.Count : 0);
    }

    public async Task<MfaSetup> BeginSetupAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await RequireUserAsync(userId);
        if (user.TwoFactorEnabled)
            throw new BusinessRuleValidationException("MfaAlreadyEnabled", "Multi-factor authentication is already enabled. Disable it before enrolling a new authenticator.");

        EnsureSucceeded(await _users.ResetAuthenticatorKeyAsync(user));
        var key = await _users.GetAuthenticatorKeyAsync(user)
                  ?? throw new InvalidOperationException("Authenticator key could not be generated.");

        var issuer = string.IsNullOrWhiteSpace(_settings.ApplicationName) ? "Enterprise SSO" : _settings.ApplicationName;
        return new MfaSetup(FormatKey(key), _totp.BuildAuthenticatorUri(issuer, user.Email ?? user.UserName ?? user.Id.ToString(), key));
    }

    public async Task<IReadOnlyList<string>> EnableAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        var user = await RequireUserAsync(userId);
        if (user.TwoFactorEnabled)
            throw new BusinessRuleValidationException("MfaAlreadyEnabled", "Multi-factor authentication is already enabled.");

        if (!await TryConsumeTotpAsync(user, code))
            throw new AuthenticationFlowException(AuthErrorCodes.InvalidMfaCode, "The verification code is invalid or expired.");

        user.TwoFactorEnabled = true;
        user.MfaEnabledAt = _clock.UtcNow;
        EnsureSucceeded(await _users.UpdateAsync(user));
        EnsureSucceeded(await _users.UpdateSecurityStampAsync(user));
        return await ReplaceRecoveryCodesAsync(user);
    }

    public async Task DisableAsync(Guid userId, string password, string code, CancellationToken cancellationToken = default)
    {
        var user = await RequireUserAsync(userId);
        if (!user.TwoFactorEnabled)
            throw new AuthenticationFlowException(AuthErrorCodes.MfaNotEnabled, "Multi-factor authentication is not enabled.");

        if (!await _users.CheckPasswordAsync(user, password ?? string.Empty))
            throw new AuthenticationFlowException(AuthErrorCodes.InvalidCredentials, "The password is incorrect.");

        if (await VerifyAsync(userId, code, cancellationToken) != MfaVerificationResult.Succeeded)
            throw new AuthenticationFlowException(AuthErrorCodes.InvalidMfaCode, "The verification code is invalid or expired.");

        user = await RequireUserAsync(userId);
        user.TwoFactorEnabled = false;
        user.MfaEnabledAt = null;
        user.LastTotpTimeStep = null;
        EnsureSucceeded(await _users.UpdateAsync(user));
        EnsureSucceeded(await _users.ResetAuthenticatorKeyAsync(user));
        await _users.RemoveAuthenticationTokenAsync(user, RecoveryProvider, RecoveryTokenName);
        EnsureSucceeded(await _users.UpdateSecurityStampAsync(user));
    }

    public async Task<IReadOnlyList<string>> RegenerateRecoveryCodesAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        var user = await RequireUserAsync(userId);
        if (!user.TwoFactorEnabled)
            throw new AuthenticationFlowException(AuthErrorCodes.MfaNotEnabled, "Multi-factor authentication is not enabled.");
        if (!await TryConsumeTotpAsync(user, code))
            throw new AuthenticationFlowException(AuthErrorCodes.InvalidMfaCode, "The verification code is invalid or expired.");

        return await ReplaceRecoveryCodesAsync(user);
    }

    public async Task AdminResetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await RequireUserAsync(userId);
        user.TwoFactorEnabled = false;
        user.MfaEnabledAt = null;
        user.LastTotpTimeStep = null;
        EnsureSucceeded(await _users.UpdateAsync(user));
        EnsureSucceeded(await _users.ResetAuthenticatorKeyAsync(user));
        await _users.RemoveAuthenticationTokenAsync(user, RecoveryProvider, RecoveryTokenName);
        EnsureSucceeded(await _users.UpdateSecurityStampAsync(user));
    }

    public async Task<MfaVerificationResult> VerifyAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null || !user.TwoFactorEnabled)
            return MfaVerificationResult.NotEnabled;

        if (await _users.IsLockedOutAsync(user))
            return MfaVerificationResult.LockedOut;

        var normalized = (code ?? string.Empty).Trim().Replace(" ", string.Empty);
        var ok = normalized.Length == 6 && normalized.All(char.IsAsciiDigit)
            ? await TryConsumeTotpAsync(user, normalized)
            : await TryRedeemRecoveryCodeAsync(user, normalized);

        if (ok)
        {
            await _users.ResetAccessFailedCountAsync(user);
            return MfaVerificationResult.Succeeded;
        }

        await _users.AccessFailedAsync(user);
        return await _users.IsLockedOutAsync(user) ? MfaVerificationResult.LockedOut : MfaVerificationResult.Invalid;
    }

    private async Task<bool> TryConsumeTotpAsync(AppUser user, string code)
    {
        var key = await _users.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(key))
            return false;

        if (!_totp.TryValidate(key, code, _clock.UtcNow, user.LastTotpTimeStep, out var step))
            return false;

        // Persisting the accepted time-step (guarded by Identity's concurrency stamp) makes each code single-use.
        user.LastTotpTimeStep = step;
        var result = await _users.UpdateAsync(user);
        return result.Succeeded;
    }

    private async Task<bool> TryRedeemRecoveryCodeAsync(AppUser user, string code)
    {
        var normalized = NormalizeRecoveryCode(code);
        if (normalized.Length == 0)
            return false;

        var hashes = await GetRecoveryHashesAsync(user);
        var hash = SecureTokens.Hash(normalized);
        if (!hashes.Remove(hash))
            return false;

        var result = await _users.SetAuthenticationTokenAsync(user, RecoveryProvider, RecoveryTokenName, string.Join(';', hashes));
        return result.Succeeded;
    }

    private async Task<IReadOnlyList<string>> ReplaceRecoveryCodesAsync(AppUser user)
    {
        var count = Math.Clamp(_settings.Security.MfaRecoveryCodeCount, 5, 20);
        var codes = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            // 60 bits of entropy, formatted XXXXXX-XXXXXX for readability.
            var raw = Base32.Encode(System.Security.Cryptography.RandomNumberGenerator.GetBytes(8))[..12];
            codes.Add($"{raw[..6]}-{raw[6..]}");
        }

        var hashes = codes.Select(c => SecureTokens.Hash(NormalizeRecoveryCode(c)));
        EnsureSucceeded(await _users.SetAuthenticationTokenAsync(user, RecoveryProvider, RecoveryTokenName, string.Join(';', hashes)));
        return codes;
    }

    private async Task<List<string>> GetRecoveryHashesAsync(AppUser user)
    {
        var value = await _users.GetAuthenticationTokenAsync(user, RecoveryProvider, RecoveryTokenName);
        return string.IsNullOrWhiteSpace(value)
            ? new List<string>()
            : value.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    private static string NormalizeRecoveryCode(string code)
        => new string((code ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

    private static string FormatKey(string key)
    {
        var chunks = Enumerable.Range(0, (key.Length + 3) / 4).Select(i => key.Substring(i * 4, Math.Min(4, key.Length - i * 4)));
        return string.Join(' ', chunks).ToLowerInvariant();
    }

    private async Task<AppUser> RequireUserAsync(Guid userId)
        => await _users.FindByIdAsync(userId.ToString()) ?? throw new UserNotFoundException(userId);

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new BusinessRuleValidationException("IdentityOperationFailed", string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
