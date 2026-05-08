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
                if (appSettings.IsProduction())
                {
                    if (string.IsNullOrWhiteSpace(config.EncryptionCertificatePath) ||
                        string.IsNullOrWhiteSpace(config.SigningCertificatePath))
                    {
                        throw new InvalidOperationException(
                            "Production environment requires EncryptionCertificatePath and SigningCertificatePath " +
                            "to be configured in AppSettings:OpenIddict section.");
                    }

                    options.AddEncryptionCertificate(LoadCertificate(
                        config.EncryptionCertificatePath,
                        config.EncryptionCertificatePassword));

                    options.AddSigningCertificate(LoadCertificate(
                        config.SigningCertificatePath,
                        config.SigningCertificatePassword));
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
