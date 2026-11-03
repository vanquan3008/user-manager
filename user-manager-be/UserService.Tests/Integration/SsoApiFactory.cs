using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using UserService.Infrastructure.Data;

namespace UserService.Tests.Integration;

/// <summary>
/// Boots the real API pipeline (auth, rate limiting, middleware, controllers) with an in-memory database.
/// Configuration is supplied through environment variables because Program binds AppSettings before Build().
/// </summary>
public sealed class SsoApiFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@integration.test";
    public const string AdminPassword = "Admin#Passw0rd!";
    public const string UserEmail = "member@integration.test";
    public const string SsoEmail = "sso@integration.test";
    public const string UserPassword = "Password123!";
    public const string ConsoleClientId = "console-app";
    public const string RedirectUri = "http://localhost:4200/auth/callback";

    private readonly string _databaseName = "sso-it-" + Guid.NewGuid();

    static SsoApiFactory()
    {
        var env = new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Development",
            ["ConnectionStrings__UserDatabase"] = "Host=unused;Database=unused",
            ["AppSettings__Environment"] = "Development",
            ["AppSettings__IsAutoMigrate"] = "true",
            ["AppSettings__ApplicationName"] = "Integration SSO",
            ["AppSettings__ApiPublicBaseUrl"] = "http://localhost",
            ["AppSettings__FrontendBaseUrl"] = "http://localhost:4200",
            ["AppSettings__Jwt__Issuer"] = "http://localhost",
            ["AppSettings__Jwt__Audience"] = "integration-api",
            ["AppSettings__Cloudinary__CloudName"] = "test",
            ["AppSettings__Cloudinary__ApiKey"] = "test",
            ["AppSettings__Cloudinary__ApiSecret"] = "test",
            ["AppSettings__Outbox__Enabled"] = "false",
            ["AppSettings__Security__MaintenanceEnabled"] = "false",
            ["AppSettings__Security__RefreshTokenReuseGraceSeconds"] = "0",
            ["AppSettings__Security__SessionValidationCacheSeconds"] = "0",
            ["AppSettings__RateLimit__RequestLimit"] = "100000",
            ["AppSettings__RateLimit__AuthRequestLimit"] = "100000",
            ["Admin__Email"] = AdminEmail,
            ["Admin__Password"] = AdminPassword,
            ["SEED_TEST_USER_NAME"] = "member",
            ["SEED_TEST_USER_EMAIL"] = UserEmail,
            ["SEED_TEST_USER_PASSWORD"] = UserPassword,
            ["SEED_SSO_USER_NAME"] = "ssomember",
            ["SEED_SSO_USER_EMAIL"] = SsoEmail,
            ["SEED_SSO_USER_PASSWORD"] = UserPassword,
            ["SEED_APP_NAME"] = ConsoleClientId,
            ["SEED_APP_DESCRIPTION"] = "Integration test application",
            ["SEED_APP_API_KEY"] = ConsoleClientId,
            ["EmailSettings__Smtp__Host"] = "localhost",
            ["EmailSettings__Smtp__Port"] = "1",
            ["EmailSettings__Sender__FromEmail"] = "noreply@integration.test"
        };
        foreach (var (key, value) in env)
            Environment.SetEnvironmentVariable(key, value);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace the Npgsql registration (EF Core keeps option callbacks as IDbContextOptionsConfiguration<T>).
            foreach (var descriptor in services.Where(d =>
                         d.ServiceType == typeof(DbContextOptions<UserDbContext>) ||
                         d.ServiceType == typeof(DbContextOptions) ||
                         d.ServiceType == typeof(IDbContextOptionsConfiguration<UserDbContext>)).ToList())
                services.Remove(descriptor);

            services.AddDbContext<UserDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
                options.UseOpenIddict<Guid>();
            });
        });
    }

    public HttpClient CreateBrowserClient()
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true, AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        return client;
    }
}

internal static class HttpTestExtensions
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static async Task<JsonElement> ReadJsonAsync(this HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(text) ? default : JsonDocument.Parse(text).RootElement.Clone();
    }

    public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string url, object body)
        => client.PostAsJsonAsync(url, body, Json);

    public static void UseBearer(this HttpClient client, string token)
        => client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public static string ErrorCode(this JsonElement json) => json.GetProperty("error").GetProperty("code").GetString()!;

    public static JsonElement JwtPayload(string jwt)
    {
        var part = jwt.Split('.')[1].Replace('-', '+').Replace('_', '/');
        part = part.PadRight(part.Length + (4 - part.Length % 4) % 4, '=');
        return JsonDocument.Parse(Convert.FromBase64String(part)).RootElement.Clone();
    }
}
