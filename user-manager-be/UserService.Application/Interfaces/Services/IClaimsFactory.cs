using System.Security.Claims;
using UserService.Application.DTOs.Auth;

namespace UserService.Application.Interfaces.Services;

public interface IClaimsFactory
{
    IReadOnlyList<Claim> CreateAccessTokenClaims(AccessTokenRequest request);
}
