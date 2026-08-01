using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Product;

public class DeleteAllFilesHandler : IRequestHandler<DeleteAllFilesCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAllFilesHandler(
        IProductRepository productRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteAllFilesCommand command, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
        if (product == null)
            throw new DomainException($"Product with ID {command.ProductId} not found");

        // Удаляем все физические файлы
        foreach (var url in product.ImageUrls.Concat(product.VideoUrls))
        {
            await _fileStorageService.DeleteFileAsync(url, ct);
        }

        // Очищаем списки
        product.ClearAllFiles();

        await _unitOfWork.SaveChangesAsync(ct);
    }
}