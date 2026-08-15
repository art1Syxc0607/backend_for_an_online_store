using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IWebHostEnvironment environment, ILogger<LocalFileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? subFolder = null,
        CancellationToken ct = default)
    {
        // 1. Определяем тип файла
        var fileType = GetFileType(contentType);
        var extension = Path.GetExtension(fileName);
        var uniqueName = $"{Guid.NewGuid():N}{extension}";

        // 2. Формируем путь с учётом типа
        var baseFolder = Path.Combine(_environment.WebRootPath, fileType);
        var targetFolder = string.IsNullOrEmpty(subFolder)
            ? baseFolder
            : Path.Combine(baseFolder, subFolder.Replace('/', Path.DirectorySeparatorChar));

        if (!Directory.Exists(targetFolder))
            Directory.CreateDirectory(targetFolder);

        // 3. Сохраняем файл
        var filePath = Path.Combine(targetFolder, uniqueName);
        using var fileStreamOutput = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamOutput, ct);

        // 4. Возвращаем URL
        var urlPath = string.IsNullOrEmpty(subFolder)
            ? $"/{fileType}/{uniqueName}"
            : $"/{fileType}/{subFolder}/{uniqueName}";

        return urlPath;
    }

    public Task DeleteFileAsync(string fileUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(fileUrl))
            return Task.CompletedTask;

        // Извлекаем путь из URL: /images/products/1/abc.jpg → images/products/1/abc.jpg
        var relativePath = fileUrl.TrimStart('/');
        var fullPath = Path.Combine(_environment.WebRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Deleted file: {FilePath}", fullPath);
        }

        return Task.CompletedTask;
    }

    public async Task DeleteMultipleFilesAsync(List<string> fileUrls, CancellationToken ct = default)
    {
        foreach (var url in fileUrls)
        {
            await DeleteFileAsync(url, ct);
        }
    }

    // ========== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ==========

    private string GetFileType(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return "images/products";

        if (contentType.StartsWith("image/"))
            return "images";

        if (contentType.StartsWith("video/"))
            return "videos";

        if (contentType == "application/pdf" ||
            contentType == "application/msword" ||
            contentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            return "documents";

        return "others";
    }

    // Дополнительный метод для проверки типа файла
    public bool IsImageFile(string contentType)
        => !string.IsNullOrEmpty(contentType) && contentType.StartsWith("image/");

    public bool IsVideoFile(string contentType)
        => !string.IsNullOrEmpty(contentType) && contentType.StartsWith("video/");

    public bool IsDocumentFile(string contentType)
        => !string.IsNullOrEmpty(contentType) && (
            contentType == "application/pdf" ||
            contentType == "application/msword" ||
            contentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        );
}