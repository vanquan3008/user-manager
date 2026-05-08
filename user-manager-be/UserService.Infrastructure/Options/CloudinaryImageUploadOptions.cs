namespace UserService.Infrastructure.Options;

internal sealed record CloudinaryImageUploadOptions(
    string Folder,
    string PublicId,
    bool Overwrite,
    bool UniqueFilename,
    bool UseFilename);
