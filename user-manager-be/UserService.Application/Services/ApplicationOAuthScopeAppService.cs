using UserService.Application.Interfaces.Services;
using UserService.Domain.Repositories;

namespace UserService.Application.Services;

public sealed class ApplicationOAuthScopeAppService : IApplicationOAuthScopeAppService
{
    private readonly IApplicationOAuthScopeRepository _applicationOAuthScopeRepository;

    public ApplicationOAuthScopeAppService(IApplicationOAuthScopeRepository applicationOAuthScopeRepository)
    {
        _applicationOAuthScopeRepository = applicationOAuthScopeRepository;
    }

    public async Task<IReadOnlyCollection<string>> GetAllowedScopeCodesAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        var result = await _applicationOAuthScopeRepository.GetAllowedScopeCodesAsync(applicationId, cancellationToken);
        return result;
    }
}
