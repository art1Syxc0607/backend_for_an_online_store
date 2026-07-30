using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Сохраняет файл и возвращает URL для доступа
    /// </summary>
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default);

    /// <summary>
    /// Удаляет файл по URL
    /// </summary>
    Task DeleteFileAsync(string fileUrl, CancellationToken ct = default);
}
