using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Product;

// WebAPI/DTOs/Product/DeleteFilesRequestDto.cs
public class DeleteFilesRequestDto
{
    public List<string> FileUrls { get; init; } = new();
}
