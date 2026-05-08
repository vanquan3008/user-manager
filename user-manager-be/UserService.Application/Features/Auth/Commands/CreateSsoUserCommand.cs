using System.Text.Json.Serialization;
using MediatR;

namespace UserService.Application.Features.Auth.Commands;
public record CreateSSOUserCommand(
    string Email,
    string Password,
    string Username,
    string FirstName,
    string LastName,
    [property: JsonPropertyName("dateOfBirth")]
    DateOnly? DateOfBirth
) : IRequest<SuccessResponse>;
