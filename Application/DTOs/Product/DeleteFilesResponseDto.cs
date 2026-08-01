using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Product;

public class DeleteFilesResponseDto
{
    public List<string> DeletedUrls { get; init; } = new();
    public List<string> NotFoundUrls { get; init; } = new();
    public List<string> FailedUrls { get; init; } = new();
}