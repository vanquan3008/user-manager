using System;
using Microsoft.AspNetCore.Identity;
using Serilog;
using UserService.Domain.Constants;
using UserService.Infrastructure.Identity;

namespace UserService.Infrastructure.Seeding;

public sealed class SystemRoleSeeder
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly ILogger _log = Log.ForContext<SystemRoleSeeder>();

    public SystemRoleSeeder(RoleManager<AppRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var roleName in RoleNames.AllRoles)
        {
            var existingRole = await _roleManager.FindByNameAsync(roleName);
            var desiredIsSystem = RoleNames.SystemRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);

            if (existingRole is null)
            {
                var role = new AppRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    IsSystemRole = desiredIsSystem
                };

                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    _log.Error("Failed to create system role {Role}: {Errors}",
                        roleName,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else if (existingRole.IsSystemRole != desiredIsSystem)
            {
                existingRole.IsSystemRole = desiredIsSystem;
                existingRole.UpdatedAt = DateTime.UtcNow;
                var updateResult = await _roleManager.UpdateAsync(existingRole);
                if (!updateResult.Succeeded)
                {
                    _log.Error("Failed to update system role {Role}: {Errors}",
                        roleName,
                        string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}

