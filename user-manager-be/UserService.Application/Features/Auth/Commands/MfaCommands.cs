using MediatR;
using UserService.Application.DTOs.Auth;

namespace UserService.Application.Features.Auth.Commands;

/// <summary>Second step of a console sign-in: exchanges an MFA challenge + code for a session.</summary>
public sealed record VerifyMfaCommand(string MfaToken, string Code) : IRequest<AuthResponse>;

/// <summary>Second step of a hosted SSO sign-in.</summary>
public sealed record VerifySsoMfaCommand(string MfaToken, string Code) : IRequest<SsoLoginResponse>;
