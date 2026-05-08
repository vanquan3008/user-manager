using Microsoft.EntityFrameworkCore;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Seeding;

public static class SubscriptionRoleTypeSeeder
{
    public static async Task SeedAsync(UserDbContext context)
    {
        if (await context.SubscriptionRoleTypes.AnyAsync())
        {
            return; 
        }

        var roleTypes = new[]
        {
            CreateRoleType(
                SubscriptionRoleTypeIds.Owner,
                SubscriptionRoleTypeIds.OwnerName,
                sortOrder: 1,
                isAdmin: true),
            CreateRoleType(
                SubscriptionRoleTypeIds.Admin,
                SubscriptionRoleTypeIds.AdminName,
                sortOrder: 2,
                isAdmin: true),
            CreateRoleType(
                SubscriptionRoleTypeIds.Member,
                SubscriptionRoleTypeIds.MemberName,
                sortOrder: 3,
                isAdmin: false)
        };

        context.SubscriptionRoleTypes.AddRange(roleTypes);
        await context.SaveChangesAsync();
    }

    private static SubscriptionRoleType CreateRoleType(Guid id, string name, int sortOrder, bool isAdmin)
    {
        var constructor = typeof(SubscriptionRoleType).GetConstructor(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            new[] { typeof(Guid), typeof(string), typeof(int), typeof(bool) },
            null);

        if (constructor == null)
            throw new InvalidOperationException("Could not find SubscriptionRoleType constructor");

        return (SubscriptionRoleType)constructor.Invoke(new object[] { id, name, sortOrder, isAdmin });
    }
}
