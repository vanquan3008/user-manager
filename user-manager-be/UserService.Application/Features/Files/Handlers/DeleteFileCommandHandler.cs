using MediatR;
using UserService.Application.Features.Files.Commands;
using UserService.Application.Services;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Files.Handlers;

public sealed class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, bool>
{
    private static readonly HashSet<string> AllowedFolders = UserService.Domain.Constants.FileFolders.AllowedFolders;

    private readonly IFileStorageService _fileStorage;

    public DeleteFileCommandHandler(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    public async Task<bool> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FileUrl))
            throw new BusinessRuleValidationException("FileUrlRequired", "fileUrl is required.");

        var fileUrl = request.FileUrl.Trim();
        if (!IsAllowedFileUrl(fileUrl))
            throw new BusinessRuleValidationException("FileUrlNotAllowed", "File URL is not allowed.");

        var deleted = await _fileStorage.DeleteFileAsync(fileUrl, cancellationToken);
        if (!deleted)
            throw new BusinessRuleValidationException("DeleteFileFailed", "Delete file failed.");

        return true;
    }

    private static bool IsAllowedFileUrl(string fileUrl)
    {
        if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
            return false;

        return AllowedFolders.Any(folder =>
            uri.AbsolutePath.Contains($"/{folder}/", StringComparison.OrdinalIgnoreCase));
    }
}

