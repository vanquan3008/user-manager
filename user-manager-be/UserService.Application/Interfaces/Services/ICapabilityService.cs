using UserService.Application.DTOs.Authorization;

namespace UserService.Application.Interfaces.Services;

public interface ICapabilityService
{
    Task<ApplicationCapabilitiesDto> GetCapabilitiesAsync(
        Guid userId,
        Guid applicationId,
        CancellationToken cancellationToken = default);
}
