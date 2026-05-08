using MediatR;

namespace UserService.Application.Features.Files.Commands;

public sealed record DeleteFileCommand(
    string FileUrl
) : IRequest<bool>;

