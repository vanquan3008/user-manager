using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Services;

public interface IApplicationAppService
{
    Task<AppApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppApplication?> UpdateLogoUrlAsync(Guid id, string? logoUrl, CancellationToken cancellationToken = default);
}
