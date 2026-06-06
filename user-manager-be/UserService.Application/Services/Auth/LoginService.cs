using UserService.Application.DTOs.Auth;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.Services.Auth;

public sealed class LoginService : ILoginService
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly AuthResponseFactory _authResponseFactory;

    public LoginService(
        IIdentityService identityService,
        IUserRepository userRepository,
        AuthResponseFactory authResponseFactory)
    {
        _identityService = identityService;
        _userRepository = userRepository;
        _authResponseFactory = authResponseFactory;
    }

    public async Task<AuthResponse> LoginAsync(
        LoginUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var (succeeded, appUser, errors) = await _identityService.LoginAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (!succeeded || appUser == null)
            throw new UnauthorizedException(string.Join(", ", errors));

        var domainUser = await _userRepository.GetByIdAsync(appUser.Id, cancellationToken);
        if (domainUser == null)
            throw new EntityNotFoundException(nameof(User), appUser.Id);

        if (domainUser.IsSSOAccount)
            throw new InvalidDomainObjectException(
                nameof(request.Email),
                request.Email,
                "This account uses the hosted SSO login flow. Please continue from the hosted auth page.");

        return await _authResponseFactory.BuildUserAuthResponseAsync(
            appUser.Id,
            request.ApplicationId,
            cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new UnauthorizedException("Invalid refresh token");

        var appUser = await _identityService.GetUserByRefreshTokenAsync(refreshToken, cancellationToken);
        if (appUser == null)
            throw new UnauthorizedException("Invalid or expired refresh token");

        var domainUser = await _userRepository.GetByIdAsync(appUser.Id, cancellationToken);
        if (domainUser?.IsSSOAccount == true)
            return await _authResponseFactory.BuildSSOAuthResponseAsync(appUser.Id, cancellationToken);

        return await _authResponseFactory.BuildUserAuthResponseAsync(appUser.Id, null, cancellationToken);
    }
}
