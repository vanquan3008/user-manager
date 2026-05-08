using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;
using UserService.Infrastructure.Options;

namespace UserService.Infrastructure.Services;

public sealed class CloudinaryFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".gif"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg",
        "image/webp",
        "image/gif"
    };

    private readonly Cloudinary _cloudinary;
    private readonly string _cloudName;

    public CloudinaryFileStorageService(
        Cloudinary cloudinary,
        Application.Configuration.CloudinaryConfiguration cloudinaryConfiguration)
    {
        _cloudinary = cloudinary;
        _cloudName = cloudinaryConfiguration.CloudName;
    }

    public async Task<UploadFileResult> UploadImageAsync(
        Stream content,
        string fileName,
        string contentType,
        long size,
        string folder,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(contentType) || !AllowedContentTypes.Contains(contentType.Trim()))
            throw new InvalidOperationException("Only image files are allowed.");
        if (size > 2 * 1024 * 1024)
            throw new InvalidOperationException("File too large. Maximum is 2MB.");
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
            throw new InvalidOperationException("Unsupported image format.");
        var safeFolder = string.Concat(folder.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')).Trim();
        if (string.IsNullOrWhiteSpace(safeFolder))
            safeFolder = "Common";

        if (content.CanSeek)
            content.Position = 0;

        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();

        var publicId = $"{safeFolder}-{Guid.NewGuid():N}";
        var options = new CloudinaryImageUploadOptions(
            Folder: safeFolder,
            PublicId: publicId,
            Overwrite: false,
            UniqueFilename: true,
            UseFilename: false);

        UploadResult uploadResult;
        try
        {
            uploadResult = await UploadAsync(bytes, fileName, options);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Cloudinary upload failed.", ex);
        }

        if (uploadResult.Error is not null)
        {
            var detail = uploadResult.Error.Message ?? uploadResult.Error.ToString();
            throw new InvalidOperationException($"Cloudinary upload failed: {detail}");
        }

        var url = uploadResult.SecureUrl?.AbsoluteUri ?? uploadResult.Url?.AbsoluteUri;
        if (string.IsNullOrWhiteSpace(url))
            throw new InvalidOperationException("Cloudinary upload did not return a valid URL.");

        var storedFileName = $"{uploadResult.PublicId}{ext}";
        return new UploadFileResult(url, storedFileName, size, contentType);
    }

    private async Task<UploadResult> UploadAsync(
        byte[] bytes,
        string fileName,
        CloudinaryImageUploadOptions options)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, new MemoryStream(bytes)),
            Folder = options.Folder,
            PublicId = options.PublicId,
            Overwrite = options.Overwrite,
            UniqueFilename = options.UniqueFilename,
            UseFilename = options.UseFilename
        };

        return await _cloudinary.UploadAsync(uploadParams);
    }

    public async Task<bool> DeleteFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        var publicId = ExtractPublicIdFromUrl(fileUrl);
        if (string.IsNullOrWhiteSpace(publicId))
            return false;

        var deleteResult = await _cloudinary.DestroyAsync(new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Image
        });

        return string.Equals(deleteResult.Result, "ok", StringComparison.OrdinalIgnoreCase)
               || string.Equals(deleteResult.Result, "not found", StringComparison.OrdinalIgnoreCase);
    }

    private string? ExtractPublicIdFromUrl(string fileUrl)
    {
        if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
            return null;

        var marker = "/upload/";
        var absolutePath = uri.AbsolutePath;
        var uploadIndex = absolutePath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (uploadIndex < 0)
            return null;

        var afterUpload = absolutePath[(uploadIndex + marker.Length)..];
        var segments = afterUpload.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
        if (segments.Count == 0)
            return null;

        var versionIndex = segments.FindIndex(s => s.Length > 1 && s[0] == 'v' && s[1..].All(char.IsDigit));
        var start = versionIndex >= 0 ? versionIndex + 1 : 0;
        if (start >= segments.Count)
            return null;

        var publicIdSegments = segments.Skip(start).ToList();
        if (publicIdSegments.Count == 0)
            return null;

        var last = publicIdSegments[^1];
        var lastWithoutExt = Path.GetFileNameWithoutExtension(last);
        publicIdSegments[^1] = lastWithoutExt;

        var publicId = string.Join("/", publicIdSegments);
        if (string.IsNullOrWhiteSpace(publicId))
            return null;
        if (!uri.Host.Contains("cloudinary.com", StringComparison.OrdinalIgnoreCase) ||
            !absolutePath.Contains($"/{_cloudName}/", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        return publicId;
    }
}
