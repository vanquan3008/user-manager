namespace UserService.Application.DTOs.Files;

public sealed record UploadFileResult(
    string Url,
    string FileName,
    long Size,
    string ContentType);
