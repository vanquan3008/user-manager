using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Data;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplySoftDeleteQueryFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            ApplySoftDeleteQueryFilter(modelBuilder, entityType);
        }

        return modelBuilder;
    }

    private static void ApplySoftDeleteQueryFilter(ModelBuilder modelBuilder, IMutableEntityType entityType)
    {
        var clrType = entityType.ClrType;
        if (!typeof(ISoftDelete).IsAssignableFrom(clrType))
        {
            return;
        }

        var parameter = Expression.Parameter(clrType, "e");
        var cast = Expression.Convert(parameter, typeof(ISoftDelete));
        var isDeleted = Expression.Property(cast, nameof(ISoftDelete.IsDeleted));
        var notDeleted = Expression.Equal(isDeleted, Expression.Constant(false));
        var lambda = Expression.Lambda(notDeleted, parameter);

        modelBuilder.Entity(clrType).HasQueryFilter(lambda);
    }
}

