using UserService.Application.Interfaces.Services;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Services;

public sealed class ApplicationAppService : IApplicationAppService
{
    private readonly IApplicationRepository _applications;
    private readonly IUnitOfWork _unitOfWork;

    public ApplicationAppService(IApplicationRepository applications, IUnitOfWork unitOfWork)
    {
        _applications = applications;
        _unitOfWork = unitOfWork;
    }

    public Task<AppApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _applications.GetByIdAsync(id, cancellationToken);

    public async Task<AppApplication?> UpdateLogoUrlAsync(Guid id, string? logoUrl, CancellationToken cancellationToken = default)
    {
        var app = await _applications.GetByIdAsync(id, cancellationToken);
        if (app is null) return null;

        app.SetLogoUrl(logoUrl != null ? UrlValue.Create(logoUrl) : null);
        await _applications.UpdateAsync(app, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return app;
    }
}
