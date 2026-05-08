namespace UserService.Application.Mapping;

public static class FileUploadMapper
{
    public static UploadFileDto ToDto(UploadFileResult result, string publicUrl) =>
        new(publicUrl, result.FileName, result.Size, result.ContentType);
}
