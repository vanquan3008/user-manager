using System.Security.Cryptography.X509Certificates;
using UserService.Application.Configuration;
using UserService.Domain.Constants;
using UserService.Infrastructure.Data;

namespace UserService.Api.Extensions;

public static class OpenIddictServiceExtensions
{
    public static IServiceCollection AddOpenIddictServer(
        this IServiceCollection services,
        AppSettings appSettings)
    {
        var config = appSettings.OpenIddict;
        
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<UserDbContext>()
                    .ReplaceDefaultEntities<Guid>();
            })
            .AddServer(options =>
            {
                options.SetTokenEndpointUris("connect/token");
                options.SetAuthorizationEndpointUris("connect/authorize");
                // Discovery and JWKS are served by WellKnownController for the platform's primary
                // OAuth/OIDC flow (/api/oauth/*); OpenIddict's own metadata endpoints are disabled
                // to avoid publishing conflicting documents at the same well-known paths.
                options.SetConfigurationEndpointUris(Array.Empty<string>());
                options.SetCryptographyEndpointUris(Array.Empty<string>());

                // Configure grant types based on settings
                if (config.AllowPasswordFlow)
                    options.AllowPasswordFlow();
                
                if (config.AllowRefreshTokenFlow)
                    options.AllowRefreshTokenFlow();
                
                if (config.AllowAuthorizationCodeFlow)
                    options.AllowAuthorizationCodeFlow();
                
                if (config.AllowMagicLinkFlow)
                    options.AllowCustomFlow(OAuthGrantTypes.MagicLink);
                options.RegisterScopes("openid", "profile", "email");
                if (config.RequireProofKeyForCodeExchange)
                    options.RequireProofKeyForCodeExchange();
                if (config.AcceptAnonymousClients)
                    options.AcceptAnonymousClients();
                if (!string.IsNullOrWhiteSpace(config.EncryptionCertificatePath) &&
                    !string.IsNullOrWhiteSpace(config.SigningCertificatePath))
                {
                    options.AddEncryptionCertificate(LoadCertificate(
                        config.EncryptionCertificatePath,
                        config.EncryptionCertificatePassword));

                    options.AddSigningCertificate(LoadCertificate(
                        config.SigningCertificatePath,
                        config.SigningCertificatePassword));
                }
                else if (appSettings.IsProduction())
                {
                    // The legacy /connect/token endpoint is optional; without certificates it runs with
                    // in-memory keys (its tokens do not survive restarts). The primary flow uses the
                    // database-backed RS256 key ring instead.
                    Serilog.Log.Warning("OpenIddict certificates are not configured; using ephemeral keys for the legacy /connect/token endpoint.");
                    options.AddEphemeralEncryptionKey();
                    options.AddEphemeralSigningKey();
                }
                else
                {
                    options.AddDevelopmentEncryptionCertificate();
                    options.AddDevelopmentSigningCertificate();
                }

                options.UseAspNetCore()
                    .EnableTokenEndpointPassthrough()
                    .EnableAuthorizationEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        return services;
    }

    private static X509Certificate2 LoadCertificate(string path, string? password)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Certificate file not found: {path}");

        try
        {
            return string.IsNullOrWhiteSpace(password)
                ? X509CertificateLoader.LoadPkcs12FromFile(path, null)
                : X509CertificateLoader.LoadPkcs12FromFile(path, password);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load certificate from {path}: {ex.Message}", ex);
        }
    }
}
