using UserService.Application.DTOs.Auth;
using UserService.Application.Interfaces.Services;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.Services.Auth;

public sealed class LoginService : ILoginService
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly AuthResponseFactory _authResponseFactory;
    private readonly ISessionService _sessions;
    private readonly IAuditLogger _audit;

    public LoginService(
        IIdentityService identityService,
        IUserRepository userRepository,
        AuthResponseFactory authResponseFactory,
        ISessionService sessions,
        IAuditLogger audit)
    {
        _identityService = identityService;
        _userRepository = userRepository;
        _authResponseFactory = authResponseFactory;
        _sessions = sessions;
        _audit = audit;
    }

    public async Task<AuthResponse> LoginAsync(
        LoginUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _identityService.LoginAsync(request.Email, request.Password, cancellationToken);
        if (!result.Succeeded || result.User == null)
        {
            await _audit.LogAsync(AuditEventTypes.LoginFailed, false,
                new AuditEntry(ActorEmail: request.Email, Reason: result.ErrorCode), cancellationToken);
            throw new AuthenticationFlowException(result.ErrorCode ?? AuthErrorCodes.InvalidCredentials, result.Message ?? "Invalid email or password.");
        }

        if (result.User.IsSSOAccount)
        {
            await _audit.LogAsync(AuditEventTypes.LoginFailed, false,
                new AuditEntry(ActorUserId: result.User.Id, ActorEmail: request.Email, Reason: AuthErrorCodes.WrongLoginEndpoint), cancellationToken);
            throw new AuthenticationFlowException(AuthErrorCodes.WrongLoginEndpoint,
                "This account uses the hosted SSO login flow. Please continue from the hosted sign-in page.");
        }

        return await _authResponseFactory.CompleteConsoleSignInAsync(
            result.User, request.ApplicationId, AuthenticationMethods.Password, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var issued = await _sessions.RotateAsync(refreshToken, cancellationToken);
        return await _authResponseFactory.BuildRefreshedResponseAsync(issued, cancellationToken);
    }
}
