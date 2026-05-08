using MediatR;
using UserService.Application.DTOs.Applications;

namespace UserService.Application.Features.Applications.Queries;

public sealed record GetApplicationByIdQuery(Guid ApplicationId) : IRequest<CreateApplicationResponse>;
