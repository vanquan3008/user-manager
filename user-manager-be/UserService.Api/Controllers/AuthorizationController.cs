using Microsoft.AspNetCore.Mvc;
using UserService.Api.Authorization.Services;

namespace UserService.Api.Controllers;

[ApiController]
[Route("connect")]
[Produces("application/json")]
public class AuthorizationController : ApiControllerBase
{
    private readonly IAuthorizationExchangeService _exchangeService;

    public AuthorizationController(IAuthorizationExchangeService exchangeService)
        => _exchangeService = exchangeService;

    [HttpPost("token")]
    [Produces("application/json")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Exchange(CancellationToken cancellationToken = default)
        => await _exchangeService.ExchangeAsync(HttpContext, cancellationToken);
}
