using MediatR;
using Application.DTOs.File;
//using Application.DTOs.Product;

namespace Application.Commands.Product;

public class AddProductCommand : IRequest<int>
{
    public string Name { get; init; }
    public decimal Price { get; init; }
    public decimal PurchasePrice { get; init; }
    public int StockQuantity { get; init; }
    public int? CategoryId { get; init; }
    public string Description { get; init; }
    //public Stream? ImageStream { get; init; }     // ← фото как поток
    //public string? ImageFileName { get; init; }
    //public string? ImageContentType { get; init; }

    public List<FileUploadDto>? Files { get; init; }
}