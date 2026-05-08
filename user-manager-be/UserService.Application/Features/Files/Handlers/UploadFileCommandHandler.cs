using MediatR;
using UserService.Application.Configuration;
using UserService.Application.DTOs.Files;
using UserService.Application.Features.Files.Commands;
using UserService.Application.Mapping;
using UserService.Application.Services;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Files.Handlers;

public sealed class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, UploadFileDto>
{
    private static readonly HashSet<string> AllowedFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "app-logos"
    };

    private readonly IFileStorageService _fileStorage;
    private readonly AppSettings _appSettings;

    public UploadFileCommandHandler(
        IFileStorageService fileStorage,
        AppSettings appSettings)
    {
        _fileStorage = fileStorage;
        _appSettings = appSettings;
    }

    public async Task<UploadFileDto> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        if (request.Content is null || !request.Content.CanRead || request.Size <= 0)
            throw new BusinessRuleValidationException("FileRequired", "File is required.");

        var folder = NormalizeFolder(request.Folder);
        if (!AllowedFolders.Contains(folder))
            throw new BusinessRuleValidationException("UploadFolderNotAllowed", "Upload folder is not allowed.");

        var result = await _fileStorage.UploadImageAsync(
            request.Content,
            request.FileName,
            request.ContentType,
            request.Size,
            folder,
            cancellationToken);

        var publicUrl = FilePublicUrlBuilder.ToClientAbsoluteUrl(result.Url, _appSettings.ApiPublicBaseUrl);

        return FileUploadMapper.ToDto(result, publicUrl);
    }

    private static string NormalizeFolder(string? folder)
        => string.IsNullOrWhiteSpace(folder) ? string.Empty : folder.Trim().ToLowerInvariant();
}

