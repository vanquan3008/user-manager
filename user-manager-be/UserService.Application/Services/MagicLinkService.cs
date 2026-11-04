using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using UserService.Application.Common;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;
using static UserService.Domain.Constants.EmailPurpose.MagicLinkPurpose;

namespace UserService.Application.Services;

public class MagicLinkService : IMagicLinkService
{
    private readonly IMagicLinkTokenRepository _tokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IClock _clock;

    public MagicLinkService(
        IMagicLinkTokenRepository tokenRepository,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IClock clock)
    {
        _tokenRepository = tokenRepository;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _clock = clock;
    }

    public async Task<(string Token, MagicLinkToken TokenEntity)> GenerateMagicLinkTokenWithoutPersistingAsync(
        Guid userId,
        string purpose,
        string? email = null,
        CancellationToken cancellationToken = default)
    {
        await _tokenRepository.InvalidateAllTokensAsync(userId, purpose, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = GenerateSecureToken();
        var magicLinkToken = MagicLinkToken.Create(
            userId,
            TokenValue.Create(SecureTokens.Hash(token)),
            purpose,
            _clock.UtcNow,
            15,
            email);

        return (token, magicLinkToken);
    }

    public async Task PersistMagicLinkTokenAsync(MagicLinkToken tokenEntity, CancellationToken cancellationToken = default)
    {
        await _tokenRepository.AddAsync(tokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> GenerateMagicLinkAsync(
        Guid userId,
        string purpose,
        string? email = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {

        await _tokenRepository.InvalidateAllTokensAsync(userId, purpose, cancellationToken);
        var token = GenerateSecureToken();
        var magicLinkToken = MagicLinkToken.Create(
            userId,
            TokenValue.Create(SecureTokens.Hash(token)),
            purpose,
            _clock.UtcNow,
            15,
            email,
            ipAddress,
            userAgent);
        await _tokenRepository.AddAsync(magicLinkToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return token;
    }
        

    public async Task<(bool IsValid, Guid? UserId)> ValidateMagicLinkAsync(
        string token,
        string purpose,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return (false, null);

        // Only the SHA-256 hash is stored, so a database leak does not expose usable links.
        var magicLinkToken = await _tokenRepository.GetValidTokenAsync(SecureTokens.Hash(token.Trim()), purpose, cancellationToken);

        if (magicLinkToken == null || !magicLinkToken.IsValid(_clock.UtcNow))
            return (false, null);

        magicLinkToken.MarkAsUsed(_clock.UtcNow);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Domain.Exceptions.ConcurrencyConflictException)
        {
            // Redeemed concurrently by another request: single use wins.
            return (false, null);
        }

        return (true, magicLinkToken.UserId);
    }

    public async Task InvalidateMagicLinksAsync(Guid userId, string purpose, CancellationToken cancellationToken = default)
    {
        await _tokenRepository.InvalidateAllTokensAsync(userId, purpose, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public string BuildMagicLinkUrl(string token, string purpose, string baseUrl, string? email = null, string? clientId = null)
    {
        var cleanBaseUrl = baseUrl.TrimEnd('/');
        var encodedToken = Uri.EscapeDataString(token);
        var encodedPurpose = Uri.EscapeDataString(purpose);
        var clientIdParam = !string.IsNullOrWhiteSpace(clientId)
            ? $"&clientId={Uri.EscapeDataString(clientId)}"
            : string.Empty;

        // PasswordlessLogin: frontend /magic-login
        if (string.Equals(purpose, PasswordlessLogin, StringComparison.OrdinalIgnoreCase))
        {
            return $"{cleanBaseUrl}/magic-login#token={encodedToken}&purpose={encodedPurpose}{clientIdParam}";
        }

        // PasswordReset: frontend /reset-password (form nhập mật khẩu mới)
        if (string.Equals(purpose, PasswordReset, StringComparison.OrdinalIgnoreCase))
        {
            var emailParam = !string.IsNullOrEmpty(email) ? $"&email={Uri.EscapeDataString(email)}" : "";
            return $"{cleanBaseUrl}/reset-password#token={encodedToken}&purpose={encodedPurpose}{emailParam}";
        }

        // EmailVerification: API endpoint chuyên verify email
        if (string.Equals(purpose, EmailVerification, StringComparison.OrdinalIgnoreCase))
        {
            return $"{cleanBaseUrl}/verify-email#token={encodedToken}";
        }

        // Fallback: the SPA callback exchanges the token via POST (tokens never travel in query strings).
        return $"{cleanBaseUrl}/magic-login#token={encodedToken}&purpose={encodedPurpose}{clientIdParam}";
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        var token = Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");

        return token;
    }
}
