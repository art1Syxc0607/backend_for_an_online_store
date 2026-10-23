using Application.Validators.Common;

namespace WebApi.Common;

/// <summary>
/// Единая проверка multipart-файлов для контроллеров товаров и отзывов.
/// </summary>
public static class FileUploadValidation
{
    public static string? Validate(IReadOnlyCollection<IFormFile>? files, bool required = false)
    {
        if (files == null || files.Count == 0)
            return required ? "No files uploaded" : null;

        if (files.Count > AllowedFileTypes.MaxFiles)
            return $"Maximum {AllowedFileTypes.MaxFiles} files allowed";

        var totalLength = 0L;

        foreach (var file in files)
        {
            if (file.Length == 0)
                return $"File '{file.FileName}' is empty";

            if (file.Length > AllowedFileTypes.MaxFileSizeBytes)
                return $"File '{file.FileName}' exceeds {AllowedFileTypes.MaxFileSizeMb}MB";

            if (!AllowedFileTypes.IsAllowed(file.ContentType))
                return $"File '{file.FileName}' has unsupported type: {file.ContentType}";

            totalLength += file.Length;

            if (totalLength > AllowedFileTypes.MaxRequestSizeBytes)
                return $"Total upload size exceeds {AllowedFileTypes.MaxRequestSizeMb}MB";
        }

        return null;
    }
}
