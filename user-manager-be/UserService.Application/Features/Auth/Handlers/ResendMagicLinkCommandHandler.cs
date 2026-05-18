using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Auth.Handlers;

public class ResendMagicLinkCommandHandler : IRequestHandler<ResendMagicLinkCommand, Unit>
{
    private readonly IMagicLinkDeliveryService _magicLinkDeliveryService;

    public ResendMagicLinkCommandHandler(IMagicLinkDeliveryService magicLinkDeliveryService)
    {
        _magicLinkDeliveryService = magicLinkDeliveryService;
    }

    public async Task<Unit> Handle(ResendMagicLinkCommand command, CancellationToken cancellationToken)
    {
        var succeeded = await _magicLinkDeliveryService.ResendMagicLinkAsync(
            command.Email,
            command.Purpose,
            cancellationToken);

        if (!succeeded)
        {
            throw new BusinessRuleValidationException("ResendMagicLinkFailed", "Failed to resend magic link");
        }

        return Unit.Value;
    }
}
