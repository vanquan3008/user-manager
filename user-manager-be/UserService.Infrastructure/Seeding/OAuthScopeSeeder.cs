using Microsoft.EntityFrameworkCore;
using Serilog;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Seeding;

public class OAuthScopeSeeder
{
    private readonly UserDbContext _db;

    public OAuthScopeSeeder(UserDbContext db)
    {
        _db = db;
    }

    private static readonly Dictionary<string, (string Code, string Description, string Category)> StandardScopes = new()
    {
        ["openId"] = ("oauth.openid", "OpenID Connect authentication", "OAuth"),
        ["profile"] = ("oauth.profile", "Access to basic profile information (name, username)", "OAuth"),
        ["email"] = ("oauth.email", "Access to user email address and email verification status", "OAuth"),
    };

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Log.Information("Starting OAuth scope seeding - {Count} scopes to process", StandardScopes.Count);
            
            var createdCount = 0;
            var updatedCount = 0;
            var skippedCount = 0;

            foreach (var (scopeName, (code, description, category)) in StandardScopes)
            {
                var permission = await _db.Permissions.FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
                
                if (permission == null)
                {
                    permission = Permission.Create(
                        code: PermissionCode.Create(code),
                        description: description,
                        category: category,
                        isSystemPermission: true
                    );
                    permission.SetScope(scopeName);
                    await _db.Permissions.AddAsync(permission, cancellationToken);
                    createdCount++;
                    Log.Information("✓ Created OAuth permission: Scope='{ScopeName}' -> Code='{Code}'", scopeName, code);
                }
                else if (permission.Scope != scopeName)
                {
                    permission.SetScope(scopeName);
                    updatedCount++;
                    Log.Information("✓ Updated OAuth scope mapping: Scope='{ScopeName}' -> Code='{Code}'", scopeName, code);
                }
                else
                {
                    skippedCount++;
                    Log.Debug("○ OAuth permission already exists: Scope='{ScopeName}' -> Code='{Code}'", scopeName, code);
                }
            }

            var changeCount = await _db.SaveChangesAsync(cancellationToken);
            
            Log.Information("OAuth scope seeding completed:");
            Log.Information("  - Created: {CreatedCount}", createdCount);
            Log.Information("  - Updated: {UpdatedCount}", updatedCount);
            Log.Information("  - Skipped: {SkippedCount}", skippedCount);
            Log.Information("  - Total changes saved: {ChangeCount}", changeCount);
            
            // Verify seeding
            var verifyCount = await _db.Permissions
                .Where(p => p.Scope != null && p.Scope != "")
                .CountAsync(cancellationToken);
            Log.Information("Total OAuth-enabled permissions in database: {Count}", verifyCount);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error seeding OAuth scopes: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<Permission?> GetPermissionByScopeAsync(string scopeName, CancellationToken cancellationToken = default)
    {
        return await _db.Permissions
            .FirstOrDefaultAsync(p => p.Scope == scopeName && p.IsActive, cancellationToken);
    }
}
