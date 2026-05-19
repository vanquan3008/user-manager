using MediatR;
using UserService.Application.DTOs.Applications;

namespace UserService.Application.Features.Applications.Commands;

public sealed record RotateApplicationClientSecretCommand(Guid ApplicationId) : IRequest<ApplicationCredentialDto>;
