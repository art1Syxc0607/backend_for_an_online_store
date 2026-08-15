// InfrastructureTests/Common/TestFileHelper.cs
using Microsoft.AspNetCore.Http;

namespace InfrastructureTests.Common;

public static class TestFileHelper
{
    public static IFormFile CreateMockFile(string fileName, string contentType, byte[]? content = null)
    {
        content ??= new byte[1024]; // 1KB default
        var stream = new MemoryStream(content);
        var file = new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
        return file;
    }

    public static IFormFile CreateImageFile(string fileName = "test.jpg")
    {
        return CreateMockFile(fileName, "image/jpeg");
    }

    public static IFormFile CreateVideoFile(string fileName = "test.mp4")
    {
        return CreateMockFile(fileName, "video/mp4");
    }

    public static IFormFile CreatePdfFile(string fileName = "test.pdf")
    {
        return CreateMockFile(fileName, "application/pdf");
    }

    public static string GetTestDirectory()
    {
        return Path.Combine(Path.GetTempPath(), "FileStorageTests", Guid.NewGuid().ToString());
    }

    public static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    public static async Task<byte[]> ReadAllBytesAsync(string path)
    {
        return await File.ReadAllBytesAsync(path);
    }
}