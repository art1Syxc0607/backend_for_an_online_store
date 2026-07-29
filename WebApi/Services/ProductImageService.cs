

using WebApi.Interfaces;

namespace WebApi.Services;

public class ProductImageService : IProductImageService
{
    private readonly IWebHostEnvironment _environment;

    public ProductImageService(IWebHostEnvironment environment)
        => _environment = environment;

    public async Task<string> SaveImageAsync(int productId, IFormFile file)
    {
        // 1. Создаем папку для товара
        var productFolder = Path.Combine(
            _environment.WebRootPath,
            "images",
            "products",
            productId.ToString()
        );

        if (!Directory.Exists(productFolder))
            Directory.CreateDirectory(productFolder);

        // 2. Генерируем уникальное имя файла
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"main_{DateTime.UtcNow:yyyyMMdd_HHmmss}{extension}";
        var filePath = Path.Combine(productFolder, fileName);

        // 3. Сохраняем файл
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        // 4. Возвращаем относительный URL
        return $"/images/products/{productId}/{fileName}";
    }
}
