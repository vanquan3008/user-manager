using Microsoft.EntityFrameworkCore;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly UserDbContext _context;

    public UnitOfWork(UserDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Surface optimistic concurrency failures (e.g. a one-time token redeemed twice
            // concurrently) as a domain exception so the application layer stays EF-agnostic.
            foreach (var entry in ex.Entries)
                entry.State = EntityState.Detached;
            throw new ConcurrencyConflictException("The resource was modified concurrently.", ex);
        }
    }
}
