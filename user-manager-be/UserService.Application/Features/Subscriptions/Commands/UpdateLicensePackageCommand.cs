using MediatR;
using UserService.Application.DTOs.Subscriptions;

namespace UserService.Application.Features.Subscriptions.Commands;

public record UpdateLicensePackageCommand(
    Guid Id,
    Guid ApplicationId,
    string Name,
    string? Description,
    IReadOnlyList<string> Features,
    int SeatLimit,
    bool IsActive,
    int DisplayOrder,
    string AssignedRole) : IRequest<LicensePackageCatalogDto>;
