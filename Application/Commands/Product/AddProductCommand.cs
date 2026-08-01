using MediatR;
using Application.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Product;

public class AddProductCommand : IRequest<int>
{
    public string Name { get; init; }
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public int? CategoryId { get; init; }
    public string Description { get; init; }
    //public Stream? ImageStream { get; init; }     // ← фото как поток
    //public string? ImageFileName { get; init; }
    //public string? ImageContentType { get; init; }

    public List<FileUploadDto>? Files { get; init; }
}