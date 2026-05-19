using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Auth.Handlers;

public class RequestPasswordlessLoginCommandHandler : IRequestHandler<RequestPasswordlessLoginCommand, Unit>
{
    private readonly IMagicLinkDeliveryService _magicLinkDeliveryService;
    private readonly IApplicationAppService _applicationService;

    public RequestPasswordlessLoginCommandHandler(IMagicLinkDeliveryService magicLinkDeliveryService, IApplicationAppService applicationService)
    {
        _magicLinkDeliveryService = magicLinkDeliveryService;
        _applicationService = applicationService;
    }

    public async Task<Unit> Handle(RequestPasswordlessLoginCommand command, CancellationToken cancellationToken)
    {
        var applicationId = await _applicationService.ResolveApplicationIdAsync(command.ClientId, cancellationToken);
        
        var succeeded = await _magicLinkDeliveryService.RequestPasswordlessLoginAsync(
            command.Email,
            applicationId,
            cancellationToken);
        
        if (!succeeded)
        {
            throw new BusinessRuleValidationException("RequestPasswordlessLoginFailed", "Failed to send passwordless login link");
        }

        return Unit.Value;
    }
}
