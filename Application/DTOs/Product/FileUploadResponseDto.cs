using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Product;

public class FileUploadResponseDto
{
    public string OriginalFileName { get; init; }
    public string FileUrl { get; init; }
    public string ContentType { get; init; }
    public long Size { get; init; }
}