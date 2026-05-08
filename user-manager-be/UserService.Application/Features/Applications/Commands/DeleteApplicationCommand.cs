using MediatR;

namespace UserService.Application.Features.Applications.Commands;

public sealed record DeleteApplicationCommand(Guid ApplicationId) : IRequest<Unit>;

