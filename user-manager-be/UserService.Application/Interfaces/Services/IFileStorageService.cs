namespace UserService.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<UploadFileResult> UploadImageAsync(
        Stream content,
        string fileName,
        string contentType,
        long size,
        string folder,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);
}
