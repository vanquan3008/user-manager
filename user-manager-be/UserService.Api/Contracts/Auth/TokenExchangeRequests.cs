namespace UserService.Api.Contracts.Auth;

public sealed record AppTokenExchangeApiRequest(
    Guid TargetApplicationId
);

public sealed record SsoAuthorizeApiRequest(
    Guid ApplicationId,
    string? ReturnUrl = null
);
