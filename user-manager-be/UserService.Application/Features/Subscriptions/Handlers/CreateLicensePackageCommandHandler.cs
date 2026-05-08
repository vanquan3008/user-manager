using System.Text.Json;
using MediatR;
using UserService.Application.DTOs.Subscriptions;
using UserService.Application.Features.Subscriptions.Commands;
using UserService.Application.Mapping;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Features.Subscriptions.Handlers;

public class CreateLicensePackageCommandHandler : IRequestHandler<CreateLicensePackageCommand, LicensePackageCatalogDto>
{
    private readonly ILicensePackageRepository _packageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLicensePackageCommandHandler(
        ILicensePackageRepository packageRepository,
        IUnitOfWork unitOfWork)
    {
        _packageRepository = packageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LicensePackageCatalogDto> Handle(CreateLicensePackageCommand command, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(command.Features);
        
        var package = LicensePackage.Create(
            Guid.NewGuid(),
            command.ApplicationId,
            command.SubscriptionPlanId,
            PlanName.Create(command.Name),
            json,
            command.AssignedRole,
            command.SeatLimit);

        package.SetDisplayOrder(command.DisplayOrder);
        if (!command.IsActive)
            package.Deactivate();

        await _packageRepository.AddAsync(package, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SubscriptionEntityMapper.ToCatalogDto(package);
    }
}

