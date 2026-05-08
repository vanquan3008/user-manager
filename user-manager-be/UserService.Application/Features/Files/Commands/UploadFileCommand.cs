using MediatR;
using System.IO;
namespace UserService.Application.Features.Files.Commands;

public sealed record UploadFileCommand(
    string FileName,
    string ContentType,
    Stream Content,
    long Size,
    string? Folder
) : IRequest<UploadFileDto>;

