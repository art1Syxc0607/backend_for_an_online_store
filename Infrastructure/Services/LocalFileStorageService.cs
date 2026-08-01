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
        string? subFolder = null,  // ← добавить!
        CancellationToken ct = default)
    {
        // 1. Генерируем уникальное имя
        var extension = Path.GetExtension(fileName);
        var uniqueName = $"{Guid.NewGuid():N}{extension}";

        // 2. Формируем путь с учетом подпапки
        var baseFolder = Path.Combine(_environment.WebRootPath, "images");
        var targetFolder = string.IsNullOrEmpty(subFolder)
            ? Path.Combine(baseFolder, "products")
            : Path.Combine(baseFolder, subFolder.Replace('/', Path.DirectorySeparatorChar));

        if (!Directory.Exists(targetFolder))
            Directory.CreateDirectory(targetFolder);

        // 3. Сохраняем файл
        var filePath = Path.Combine(targetFolder, uniqueName);
        using var fileStreamOutput = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamOutput, ct);

        // 4. Возвращаем URL
        var urlPath = string.IsNullOrEmpty(subFolder) 
            ? $"/images/products/{uniqueName}"
            : $"/images/{subFolder}/{uniqueName}";

        return urlPath;
    }

    public Task DeleteFileAsync(string fileUrl, CancellationToken ct = default)
    {
        var fileName = Path.GetFileName(fileUrl);
        var filePath = Path.Combine(_environment.WebRootPath, "images", "products", fileName);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    public async Task DeleteMultipleFilesAsync(List<string> fileUrls, CancellationToken ct = default)
    {
        foreach (var url in fileUrls)
        {
            await DeleteFileAsync(url, ct);
        }
    }
}
