namespace UserService.Domain.Repositories;

/// <summary>
/// Unit of Work to coordinate repository changes and persistence.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
