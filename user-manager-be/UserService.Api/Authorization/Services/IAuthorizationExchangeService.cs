using Microsoft.AspNetCore.Mvc;

namespace UserService.Api.Authorization.Services;

public interface IAuthorizationExchangeService
{
    Task<IActionResult> ExchangeAsync(HttpContext httpContext, CancellationToken cancellationToken = default);
}

