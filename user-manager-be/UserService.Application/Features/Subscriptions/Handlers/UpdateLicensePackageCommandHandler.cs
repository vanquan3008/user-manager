using System.Text.Json;
using MediatR;
using UserService.Application.DTOs.Subscriptions;
using UserService.Application.Features.Subscriptions.Commands;
using UserService.Application.Mapping;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Subscriptions.Handlers;

public class UpdateLicensePackageCommandHandler : IRequestHandler<UpdateLicensePackageCommand, LicensePackageCatalogDto>
{
    private readonly ILicensePackageRepository _packageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLicensePackageCommandHandler(
        ILicensePackageRepository packageRepository,
        IUnitOfWork unitOfWork)
    {
        _packageRepository = packageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LicensePackageCatalogDto> Handle(UpdateLicensePackageCommand command, CancellationToken cancellationToken)
    {
        var package = await _packageRepository.GetByIdAsync(command.Id, cancellationToken);
        if (package == null)
            throw new BusinessRuleValidationException("LicensePackageNotFound", "License package not found");

        if (package.ApplicationId != command.ApplicationId)
            throw new BusinessRuleValidationException("ApplicationMismatch", "ApplicationId does not match this license package.");

        var json = JsonSerializer.Serialize(command.Features);
        
        package.Update(PlanName.Create(command.Name.Trim()), json, command.SeatLimit, command.AssignedRole);
        package.SetDisplayOrder(command.DisplayOrder);
        
        if (command.IsActive)
            package.Activate();
        else
            package.Deactivate();

        await _packageRepository.UpdateAsync(package, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SubscriptionEntityMapper.ToCatalogDto(package);
    }
}
