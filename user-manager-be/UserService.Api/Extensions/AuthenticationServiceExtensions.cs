using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Constants;

namespace UserService.Api.Extensions;

public static class AuthenticationServiceExtensions
{
    public const string BearerScheme = "Bearer";

    /// <summary>
    /// Validates RS256 access tokens against the signing key ring (supports key rotation via <c>kid</c>)
    /// and rejects tokens whose server-side session (<c>sid</c>) has been revoked.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        AppSettings appSettings)
    {
        var jwt = appSettings.Jwt;
        if (string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience))
            throw new InvalidOperationException("AppSettings:Jwt:Issuer and AppSettings:Jwt:Audience must be configured.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = BearerScheme;
            options.DefaultChallengeScheme = BearerScheme;
        })
        .AddJwtBearer(BearerScheme, options =>
        {
            options.MapInboundClaims = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 },
                ClockSkew = TimeSpan.FromSeconds(30),
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Name
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var sid = context.Principal?.FindFirst(JwtClaimNames.SessionId)?.Value;
                    if (string.IsNullOrEmpty(sid))
                        return;

                    var validator = context.HttpContext.RequestServices.GetRequiredService<ISessionValidator>();
                    if (!Guid.TryParse(sid, out var sessionId) ||
                        !await validator.IsActiveAsync(sessionId, context.HttpContext.RequestAborted))
                    {
                        context.Fail("The session for this token has been revoked or has expired.");
                    }
                }
            };
        });

        // Resolve keys at validation time so rotated keys are picked up without restarts.
        services.AddOptions<JwtBearerOptions>(BearerScheme)
            .Configure<ISigningKeyProvider>((options, keys) =>
                options.TokenValidationParameters.IssuerSigningKeyResolver = (_, _, _, _) => keys.GetValidationKeys());

        return services;
    }
}
