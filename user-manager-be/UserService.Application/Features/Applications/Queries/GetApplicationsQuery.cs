using MediatR;

namespace UserService.Application.Features.Applications.Queries;

public sealed record GetApplicationsQuery(Guid? UserId, bool IsAdmin)
	: IRequest<IReadOnlyList<CreateApplicationResponse>>;

