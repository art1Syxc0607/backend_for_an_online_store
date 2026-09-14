// InfrastructureTests/Services/LocalFileStorageServiceTests.cs
using FluentAssertions;
using Infrastructure.Services;
using InfrastructureTests.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
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
        Directory.CreateDirectory(_testRootPath);

        _environment = new TestWebHostEnvironment(_testRootPath);

        // ✅ ПРАВИЛЬНО: создаем мок IHttpContextAccessor
        // HttpContext = null (по умолчанию), поэтому GetFileUrlAsync вернет относительный URL
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        _fileStorageService = new LocalFileStorageService(
            _environment,
            httpContextAccessorMock.Object, // ← Мок, а не null!
            NullLogger<LocalFileStorageService>.Instance);
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
        var content = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        var file = TestFileHelper.CreateMockFile(fileName, contentType, content);

        // Act
        var result = await _fileStorageService.UploadFileAsync(
            file.OpenReadStream(),
            file.FileName,
            file.ContentType,
            "products/1");

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
        var file = TestFileHelper.CreateImageFile("test.jpg");

        var result = await _fileStorageService.UploadFileAsync(
            file.OpenReadStream(), file.FileName, file.ContentType, "products/999");

        var folderPath = Path.Combine(_testRootPath, "images", "products", "999");
        Directory.Exists(folderPath).Should().BeTrue();
    }

    [Fact]
    public async Task UploadFileAsync_ShouldGenerateUniqueFileName()
    {
        var file = TestFileHelper.CreateImageFile("photo.jpg");
        var secondFile = TestFileHelper.CreateImageFile("photo.jpg");

        var result1 = await _fileStorageService.UploadFileAsync(
            file.OpenReadStream(), file.FileName, file.ContentType, "products/1");

        var result2 = await _fileStorageService.UploadFileAsync(
            secondFile.OpenReadStream(), secondFile.FileName, secondFile.ContentType, "products/1");

        result1.Should().NotBe(result2);
        Path.GetFileName(result1).Should().NotBe(Path.GetFileName(result2));
    }

    [Fact]
    public async Task UploadFileAsync_WithDifferentSubFolders_ShouldSaveInCorrectFolders()
    {
        var imageFile = TestFileHelper.CreateImageFile("photo.jpg");
        var videoFile = TestFileHelper.CreateVideoFile("video.mp4");

        var imageUrl = await _fileStorageService.UploadFileAsync(
            imageFile.OpenReadStream(), imageFile.FileName, imageFile.ContentType, "products/1/images");

        var videoUrl = await _fileStorageService.UploadFileAsync(
            videoFile.OpenReadStream(), videoFile.FileName, videoFile.ContentType, "products/1/videos");

        imageUrl.Should().Contain("/images/products/1/images/");
        videoUrl.Should().Contain("/videos/products/1/videos/");

        var imagePath = Path.Combine(_testRootPath, imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        var videoPath = Path.Combine(_testRootPath, videoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        File.Exists(imagePath).Should().BeTrue();
        File.Exists(videoPath).Should().BeTrue();
    }

    [Fact]
    public async Task UploadFileAsync_WhenSubFolderIsNull_ShouldSaveInDefaultFolder()
    {
        var file = TestFileHelper.CreateImageFile("test.jpg");

        var result = await _fileStorageService.UploadFileAsync(
            file.OpenReadStream(), file.FileName, file.ContentType, null);

        result.Should().StartWith("/images/");

        var filePath = Path.Combine(_testRootPath, result.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task UploadFileAsync_WithMultipleFiles_ShouldSaveAll()
    {
        var files = new[]
        {
            TestFileHelper.CreateImageFile("img1.jpg"),
            TestFileHelper.CreateImageFile("img2.png"),
            TestFileHelper.CreateImageFile("img3.gif")
        };

        var results = new List<string>();
        foreach (var file in files)
        {
            var result = await _fileStorageService.UploadFileAsync(
                file.OpenReadStream(), file.FileName, file.ContentType, "products/test");
            results.Add(result);
        }

        results.Should().HaveCount(3);
        foreach (var result in results)
        {
            var savedPath = Path.Combine(_testRootPath, result.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            File.Exists(savedPath).Should().BeTrue();
        }
    }

    // ========== 2. УДАЛЕНИЕ ФАЙЛОВ ==========

    [Fact]
    public async Task DeleteFileAsync_WhenFileExists_ShouldDeleteFile()
    {
        var file = TestFileHelper.CreateImageFile("test.jpg");
        var url = await _fileStorageService.UploadFileAsync(
            file.OpenReadStream(), file.FileName, file.ContentType, "products/1");

        var filePath = Path.Combine(_testRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        File.Exists(filePath).Should().BeTrue();

        await _fileStorageService.DeleteFileAsync(url);

        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFileAsync_WhenFileDoesNotExist_ShouldNotThrow()
    {
        var nonExistentUrl = "/images/products/999/nonexistent.jpg";

        Func<Task> act = async () => await _fileStorageService.DeleteFileAsync(nonExistentUrl);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteFileAsync_WhenUrlIsNull_ShouldNotThrow()
    {
        Func<Task> act = async () => await _fileStorageService.DeleteFileAsync(null!);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteFileAsync_WhenUrlIsEmpty_ShouldNotThrow()
    {
        Func<Task> act = async () => await _fileStorageService.DeleteFileAsync("");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteMultipleFilesAsync_ShouldDeleteAll()
    {
        var urls = new List<string>();
        for (int i = 1; i <= 3; i++)
        {
            var file = TestFileHelper.CreateImageFile($"img{i}.jpg");
            var url = await _fileStorageService.UploadFileAsync(
                file.OpenReadStream(), file.FileName, file.ContentType, "products/multiple");
            urls.Add(url);
        }

        foreach (var url in urls)
        {
            await _fileStorageService.DeleteFileAsync(url);
        }

        foreach (var url in urls)
        {
            var filePath = Path.Combine(_testRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            File.Exists(filePath).Should().BeFalse();
        }
    }

    // ========== 3. ИНТЕГРАЦИЯ С РАЗНЫМИ ТИПАМИ ФАЙЛОВ ==========

    [Theory]
    [InlineData("image/jpeg", ".jpg")]
    [InlineData("image/png", ".png")]
    [InlineData("image/webp", ".webp")]
    [InlineData("image/gif", ".gif")]
    [InlineData("video/mp4", ".mp4")]
    [InlineData("video/webm", ".webm")]
    [InlineData("application/pdf", ".pdf")]
    public async Task UploadFileAsync_WithDifferentContentTypes_ShouldPreserveExtension(
        string contentType, string expectedExtension)
    {
        var fileName = $"test{expectedExtension}";
        var file = TestFileHelper.CreateMockFile(fileName, contentType);

        var result = await _fileStorageService.UploadFileAsync(
            file.OpenReadStream(), file.FileName, file.ContentType, "products/types");

        result.Should().EndWith(expectedExtension);

        var filePath = Path.Combine(_testRootPath, result.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        File.Exists(filePath).Should().BeTrue();
    }

    // ========== 4. ОБРАБОТКА ОШИБОК ==========

    [Fact]
    public async Task UploadFileAsync_WithEmptyStream_ShouldSaveEmptyFile()
    {
        var stream = new MemoryStream();
        var emptyFile = new FormFile(stream, 0, stream.Length, "file", "empty.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        var result = await _fileStorageService.UploadFileAsync(
            emptyFile.OpenReadStream(), emptyFile.FileName, emptyFile.ContentType, "products/empty");

        var filePath = Path.Combine(_testRootPath, result.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        File.Exists(filePath).Should().BeTrue();
        var content = await File.ReadAllBytesAsync(filePath);
        content.Should().BeEmpty();
    }

    [Fact]
    public async Task UploadFileAsync_WithLargeFile_ShouldSaveCorrectly()
    {
        var content = new byte[10 * 1024 * 1024]; // 10MB
        new Random().NextBytes(content);
        var file = TestFileHelper.CreateMockFile("large.bin", "application/octet-stream", content);

        var result = await _fileStorageService.UploadFileAsync(
            file.OpenReadStream(), file.FileName, file.ContentType, "products/large");

        var filePath = Path.Combine(_testRootPath, result.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
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
        if (!Directory.Exists(rootPath))
            Directory.CreateDirectory(rootPath);

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

// Сводка теста: всего: 20; сбой: 0; успешно: 20; пропущено: 0; длительность: 1,6 с