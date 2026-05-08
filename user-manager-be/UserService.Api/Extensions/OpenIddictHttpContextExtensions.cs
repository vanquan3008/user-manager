using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using OpenIddict.Abstractions;

namespace UserService.Api.Extensions;

public static class OpenIddictHttpContextExtensions
{
    public static OpenIddictRequest GetRequiredOpenIddictServerRequest(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var request = httpContext.GetOpenIddictServerRequest();
        return request ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
    }
}
