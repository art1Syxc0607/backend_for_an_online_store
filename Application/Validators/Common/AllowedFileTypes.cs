namespace Application.Validators.Common;

/// <summary>
/// Ограничения на загружаемые файлы, общие для товаров и отзывов.
/// Разрешены только медиафайлы: изображения и видео.
/// </summary>
public static class AllowedFileTypes
{
    public const int MaxFiles = 8;

    public const int MaxFileSizeMb = 100;
    public const long MaxFileSizeBytes = MaxFileSizeMb * 1024L * 1024L;

    public const int MaxRequestSizeMb = 256;
    public const long MaxRequestSizeBytes = MaxRequestSizeMb * 1024L * 1024L;

    private static readonly HashSet<string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        // Images
        "image/jpeg", "image/png", "image/webp", "image/gif", "image/svg+xml",
        // Videos
        "video/mp4", "video/webm", "video/ogg", "video/quicktime"
    };

    public static bool IsAllowed(string? contentType)
        => !string.IsNullOrWhiteSpace(contentType) && ContentTypes.Contains(contentType);
}
