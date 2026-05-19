using Microsoft.EntityFrameworkCore;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Seeding;

public sealed class FeatureSeeder
{
    private readonly UserDbContext _dbContext;

    public FeatureSeeder(UserDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var existingCodes = await _dbContext.Features
            .AsNoTracking()
            .Select(feature => feature.Code)
            .ToListAsync(cancellationToken);

        var existing = existingCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var code in FeatureCodes.All)
        {
            if (existing.Contains(code))
                continue;

            await _dbContext.Features.AddAsync(
                Feature.Create(Guid.NewGuid(), code, ToDisplayName(code), string.Empty),
                cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string ToDisplayName(string code)
    {
        return string.Join(
            ' ',
            code.Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant()));
    }
}
