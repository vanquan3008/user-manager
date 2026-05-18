using UserService.Domain.Exceptions;
using static UserService.Domain.Constants.EmailPurpose.MagicLinkPurpose;

namespace UserService.Application.Services.Auth;

public sealed class RegistrationService : IRegistrationService
{
    private readonly IIdentityService _identityService;
    private readonly IRegistrationWorkflowService _registrationWorkflowService;
    private readonly IApplicationAppService _applicationService;
    private readonly IApplicationAccessService _applicationAccessService;
    private readonly IMagicLinkDeliveryService _magicLinkDeliveryService;
    private readonly AuthResponseFactory _authResponseFactory;

    public RegistrationService(
        IIdentityService identityService,
        IRegistrationWorkflowService registrationWorkflowService,
        IApplicationAppService applicationService,
        IApplicationAccessService applicationAccessService,
        IMagicLinkDeliveryService magicLinkDeliveryService,
        AuthResponseFactory authResponseFactory)
    {
        _identityService = identityService;
        _registrationWorkflowService = registrationWorkflowService;
        _applicationService = applicationService;
        _applicationAccessService = applicationAccessService;
        _magicLinkDeliveryService = magicLinkDeliveryService;
        _authResponseFactory = authResponseFactory;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureRegistrationAllowedAsync(request, cancellationToken);

        var registeredUserId = await _registrationWorkflowService.CreateUserAsync(request, cancellationToken);

        var scopedApplicationId = await ResolveScopedApplicationIdAsync(
            registeredUserId,
            request.ApplicationId,
            cancellationToken);

        var response = await _authResponseFactory.BuildUserAuthResponseAsync(
            registeredUserId,
            scopedApplicationId,
            cancellationToken);

        return response with { RequiresEmailVerification = true };
    }

    private async Task EnsureRegistrationAllowedAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var existingUser = await _identityService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            if (!existingUser.EmailConfirmed)
            {
                await _magicLinkDeliveryService.ResendMagicLinkAsync(
                    request.Email,
                    EmailVerification,
                    cancellationToken);

                throw new VerificationEmailResentException(
                    "Email is not confirmed. A new verification email has been sent.");
            }

            throw new DuplicateUserException("email", request.Email);
        }

        if (!request.ApplicationId.HasValue)
            return;

        var application = await _applicationService.GetByIdAsync(request.ApplicationId.Value, cancellationToken);
        if (application == null || !application.IsActive)
            throw new ApplicationInactiveException(request.ApplicationId);
    }

    private async Task<Guid?> ResolveScopedApplicationIdAsync(
        Guid userId,
        Guid? requestedApplicationId,
        CancellationToken cancellationToken)
    {
        if (!requestedApplicationId.HasValue)
            return null;

        var roles = await _identityService.GetUserRolesAsync(userId, cancellationToken);
        var hasScopedAccess = await _applicationAccessService.HasAccessAsync(
            userId,
            requestedApplicationId.Value,
            roles,
            cancellationToken);

        return hasScopedAccess
            ? requestedApplicationId
            : null;
    }
}
