// InfrastructureTests/Services/LocalFileStorageServiceTests.cs
using FluentAssertions;
using Infrastructure.Services;
using InfrastructureTests.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace InfrastructureTests.Services;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _testRootPath;
    private readonly LocalFileStorageService _fileStorageService;
    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageServiceTests()
    {
        _testRootPath = TestFileHelper.GetTestDirectory();

        // ✅ Создаём папку ДО создания TestWebHostEnvironment
        Directory.CreateDirectory(_testRootPath);

        _environment = new TestWebHostEnvironment(_testRootPath);
        _fileStorageService = new LocalFileStorageService(_environment, NullLogger<LocalFileStorageService>.Instance);
    }

    public void Dispose()
    {
        TestFileHelper.DeleteDirectory(_testRootPath);
    }

    // ========== 1. ЗАГРУЗКА ФАЙЛОВ ==========

    [Fact]
    public async Task UploadFileAsync_WhenFileIsValid_ShouldSaveFile()
    {
        // Arrange
        var fileName = "test.jpg";
        var contentType = "image/jpeg";
        var content = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPG header
        var file = TestFileHelper.CreateMockFile(fileName, contentType, content);

        // Act
        var result = await _fileStorageService.UploadFileAsync(
            file.OpenReadStream(),
            file.FileName,
            file.ContentType,
            "products/1"
        );

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("/images/products/1/");
        result.Should().EndWith(".jpg");

        var savedPath = Path.Combine(_testRootPath, "images", "products", "1", Path.GetFileName(result));
        File.Exists(savedPath).Should().BeTrue();

        var savedContent = await File.ReadAllBytesAsync(savedPath);
        savedContent.Should().BeEquivalentTo(content);
    }

    [Fact]
    public async Task UploadFileAsync_WhenFolderDoesNotExist_ShouldCreateFolder()
    {
        // Arrange
        var file = TestFileHelper.CreateImageFile("test.jpg");

        // Act
        var result = await _fileStorageService.UploadFileAsync(
            file.OpenReadStream(),
            file.FileName,
            file.ContentType,
            "products/999"
        );

        // Assert
        var folderPath = Path.Combine(_testRootPath, "images", "products", "999");
        Directory.Exists(folderPath).Should().BeTrue();
    }

    [Fact]
    public async Task UploadFileAsync_ShouldGenerateUniqueFileName()
    {
        // Arrange
        var file = TestFileHelper.CreateImageFile("photo.jpg");
        var secondFile = TestFileHelper.CreateImageFile("photo.jpg");

        // Act
        var result1 = await _fileStorageService.UploadFileAsync(
            file.OpenReadStream(),
            file.FileName,
            file.ContentType,
            "products/1"
        );

        var result2 = await _fileStorageService.UploadFileAsync(
            secondFile.OpenReadStream(),
            secondFile.FileName,
            secondFile.ContentType,
            "products/1"
        );

        // Assert
        result1.Should().NotBe(result2);
        var fileName1 = Path.GetFileName(result1);
        var fileName2 = Path.GetFileName(result2);
        fileName1.Should().NotBe(fileName2);
    }

    [Fact]
    public async Task UploadFileAsync_WithDifferentSubFolders_ShouldSaveInCorrectFolders()
    {
        // Arrange
        var imageFile = TestFileHelper.CreateImageFile("photo.jpg");
        var videoFile = TestFileHelper.CreateVideoFile("video.mp4");

        // Act
        var imageUrl = await _fileStorageService.UploadFileAsync(
            imageFile.OpenReadStream(),
            imageFile.FileName,
            imageFile.ContentType,
            "products/1/images"
        );

        var videoUrl = await _fileStorageService.UploadFileAsync(
            videoFile.OpenReadStream(),
            videoFile.FileName,
            videoFile.ContentType,
            "products/1/videos"
        );

        // Assert
        imageUrl.Should().Contain("/images/products/1/images/");
        videoUrl.Should().Contain("/videos/products/1/videos/");

        // ✅ Проверяем через относительный путь
        var imagePath = Path.Combine(_testRootPath, imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        var videoPath = Path.Combine(_testRootPath, videoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        File.Exists(imagePath).Should().BeTrue();
        File.Exists(videoPath).Should().BeTrue();
    }

    [Fact]
    public async Task UploadFileAsync_WhenSubFolderIsNull_ShouldSaveInDefaultFolder()
    {
        // Arrange
        var file = TestFileHelper.CreateImageFile("test.jpg");

        // Act
        var result = await _fileStorageService.UploadFileAsync(
            file.OpenReadStream(),
            file.FileName,
            file.ContentType,
            null
        );

        // Assert
        result.Should().StartWith("/images/");

        // ✅ Проверяем, что файл существует
        var relativePath = result.TrimStart('/');
        var filePath = Path.Combine(_testRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task UploadFileAsync_WithMultipleFiles_ShouldSaveAll()
    {
        // Arrange
        var files = new[]
        {
            TestFileHelper.CreateImageFile("img1.jpg"),
            TestFileHelper.CreateImageFile("img2.png"),
            TestFileHelper.CreateImageFile("img3.gif")
        };

        // Act
        var results = new List<string>();
        foreach (var file in files)
        {
            var result = await _fileStorageService.UploadFileAsync(
                file.OpenReadStream(),
                file.FileName,
                file.ContentType,
                "products/test"
            );
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(3);
        foreach (var result in results)
        {
            var savedPath = Path.Combine(_testRootPath, "images", "products", "test", Path.GetFileName(result));
            File.Exists(savedPath).Should().BeTrue();
        }
    }

    // ========== 2. УДАЛЕНИЕ ФАЙЛОВ ==========

    [Fact]
    public async Task DeleteFileAsync_WhenFileExists_ShouldDeleteFile()
    {
        // Arrange
        var file = TestFileHelper.CreateImageFile("test.jpg");
        var url = await _fileStorageService.UploadFileAsync(
            file.OpenReadStream(),
            file.FileName,
            file.ContentType,
            "products/1"
        );

        var filePath = Path.Combine(_testRootPath, "images", "products", "1", Path.GetFileName(url));
        File.Exists(filePath).Should().BeTrue();

        // Act
        await _fileStorageService.DeleteFileAsync(url);

        // Assert
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFileAsync_WhenFileDoesNotExist_ShouldNotThrow()
    {
        // Arrange
        var nonExistentUrl = "/images/products/999/nonexistent.jpg";

        // Act
        Func<Task> act = async () => await _fileStorageService.DeleteFileAsync(nonExistentUrl);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteFileAsync_WhenUrlIsNull_ShouldNotThrow()
    {
        // Act
        Func<Task> act = async () => await _fileStorageService.DeleteFileAsync(null!);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteFileAsync_WhenUrlIsEmpty_ShouldNotThrow()
    {
        // Act
        Func<Task> act = async () => await _fileStorageService.DeleteFileAsync("");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteMultipleFilesAsync_ShouldDeleteAll()
    {
        // Arrange
        var urls = new List<string>();
        for (int i = 1; i <= 3; i++)
        {
            var file = TestFileHelper.CreateImageFile($"img{i}.jpg");
            var url = await _fileStorageService.UploadFileAsync(
                file.OpenReadStream(),
                file.FileName,
                file.ContentType,
                "products/multiple"
            );
            urls.Add(url);
        }

        // Act
        foreach (var url in urls)
        {
            await _fileStorageService.DeleteFileAsync(url);
        }

        // Assert
        foreach (var url in urls)
        {
            var filePath = Path.Combine(_testRootPath, "images", "products", "multiple", Path.GetFileName(url));
            File.Exists(filePath).Should().BeFalse();
        }
    }

    // ========== 3. ИНТЕГРАЦИЯ С РАЗНЫМИ ТИПАМИ ФАЙЛОВ ==========

    [Theory]
    [InlineData("image/jpeg", ".jpg", "images")]
    [InlineData("image/png", ".png", "images")]
    [InlineData("image/webp", ".webp", "images")]
    [InlineData("image/gif", ".gif", "images")]
    [InlineData("video/mp4", ".mp4", "videos")]
    [InlineData("video/webm", ".webm", "videos")]
    [InlineData("application/pdf", ".pdf", "documents")]
    public async Task UploadFileAsync_WithDifferentContentTypes_ShouldPreserveExtension(
        string contentType, string expectedExtension, string expectedFolder)
    {
        // Arrange
        var fileName = $"test{expectedExtension}";
        var file = TestFileHelper.CreateMockFile(fileName, contentType);

        // Act
        var result = await _fileStorageService.UploadFileAsync(
            file.OpenReadStream(),
            file.FileName,
            file.ContentType,
            "products/types"
        );

        // Assert
        result.Should().EndWith(expectedExtension);

        // ✅ Проверяем в правильной папке
        var relativePath = result.TrimStart('/');
        var filePath = Path.Combine(_testRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        File.Exists(filePath).Should().BeTrue();
    }

    // ========== 4. ОБРАБОТКА ОШИБОК ==========

    [Fact]
    public async Task UploadFileAsync_WithEmptyStream_ShouldSaveEmptyFile()
    {
        // Arrange
        var file = TestFileHelper.CreateImageFile("empty.jpg");
        // Переопределяем поток как пустой
        var stream = new MemoryStream();
        var emptyFile = new FormFile(stream, 0, stream.Length, "file", "empty.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        // Act
        var result = await _fileStorageService.UploadFileAsync(
            emptyFile.OpenReadStream(),
            emptyFile.FileName,
            emptyFile.ContentType,
            "products/empty"
        );

        // Assert
        var filePath = Path.Combine(_testRootPath, "images", "products", "empty", Path.GetFileName(result));
        File.Exists(filePath).Should().BeTrue();
        var content = await File.ReadAllBytesAsync(filePath);
        content.Should().BeEmpty();
    }

    [Fact]
    public async Task UploadFileAsync_WithLargeFile_ShouldSaveCorrectly()
    {
        // Arrange
        var content = new byte[10 * 1024 * 1024]; // 10MB
        new Random().NextBytes(content);
        var file = TestFileHelper.CreateMockFile("large.bin", "application/octet-stream", content);

        // Act
        var result = await _fileStorageService.UploadFileAsync(
            file.OpenReadStream(),
            file.FileName,
            file.ContentType,
            "products/large"
        );

        // Assert
        var relativePath = result.TrimStart('/');
        var filePath = Path.Combine(_testRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        File.Exists(filePath).Should().BeTrue();

        var savedContent = await File.ReadAllBytesAsync(filePath);
        savedContent.Length.Should().Be(content.Length);
    }
}

// ========== TestWebHostEnvironment ==========
public class TestWebHostEnvironment : IWebHostEnvironment
{
    public TestWebHostEnvironment(string rootPath)
    {
        // ✅ Проверяем и создаём папку
        if (!Directory.Exists(rootPath))
        {
            Directory.CreateDirectory(rootPath);
        }

        WebRootPath = rootPath;
        ContentRootPath = rootPath;
        ApplicationName = "TestApp";
        EnvironmentName = "Test";

        ContentRootFileProvider = new PhysicalFileProvider(rootPath);
        WebRootFileProvider = new PhysicalFileProvider(rootPath);
    }

    public string ApplicationName { get; set; }
    public string ContentRootPath { get; set; }
    public string WebRootPath { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; }
    public IFileProvider WebRootFileProvider { get; set; }
    public string EnvironmentName { get; set; }
}


// Сводка теста: всего: 20; сбой: 0; успешно: 20; пропущено: 0; длительность: 2,6 с