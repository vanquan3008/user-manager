using UserService.Domain.Entities;
using UserService.Application.DTOs.Applications;
using UserService.Application.Features.Applications.Commands;

namespace UserService.Application.Interfaces.Services;

public interface IApplicationAppService
{
    Task<AppApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CreateApplicationResponse> CreateAsync(CreateApplicationCommand command, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<ApplicationCredentialDto> GetCredentialsAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<ApplicationCredentialDto> RotateClientSecretAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<AppApplication?> UpdateLogoUrlAsync(Guid id, string? logoUrl, CancellationToken cancellationToken = default);
    Task<Guid?> ResolveApplicationIdAsync(string? clientId, CancellationToken cancellationToken = default);
}
