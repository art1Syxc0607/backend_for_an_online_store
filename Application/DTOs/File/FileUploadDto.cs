using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.File;

public class FileUploadDto
{
    public Stream Stream { get; init; }
    public string FileName { get; init; }
    public string ContentType { get; init; }
    public long Length { get; init; }
}