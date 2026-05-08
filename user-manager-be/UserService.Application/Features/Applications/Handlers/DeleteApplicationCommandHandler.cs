using MediatR;
using UserService.Application.Features.Applications.Commands;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Applications.Handlers;

public sealed class DeleteApplicationCommandHandler : IRequestHandler<DeleteApplicationCommand, Unit>
{
    private readonly IApplicationRepository _applications;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteApplicationCommandHandler(
        IApplicationRepository applications,
        IUnitOfWork unitOfWork)
    {
        _applications = applications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteApplicationCommand request, CancellationToken cancellationToken)
    {
        var app = await _applications.GetByIdAsync(request.ApplicationId, cancellationToken);
        if (app is null)
            throw new ApplicationNotFoundException(request.ApplicationId);

        // Soft-delete to keep referential integrity for memberships/subscriptions.
        app.Deactivate();
        app.Delete();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

