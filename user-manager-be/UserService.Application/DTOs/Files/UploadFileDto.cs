namespace UserService.Application.DTOs.Files;

public sealed record UploadFileDto(
    string Url,
    string FileName,
    long Size,
    string ContentType);
