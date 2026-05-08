using MediatR;

namespace UserService.Application.Features.Applications.Commands;

public sealed record UpdateApplicationLogoCommand(
    Guid ApplicationId,
    string? LogoUrl
) : IRequest<CreateApplicationResponse>;

