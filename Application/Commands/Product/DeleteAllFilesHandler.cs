using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<DeleteAllFilesHandler> _logger;

    public DeleteAllFilesHandler(
        IProductRepository productRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ILogger<DeleteAllFilesHandler> logger)
    {
        _productRepository = productRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DeleteAllFilesCommand command, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
        if (product == null)
            throw new DomainException($"Product with ID {command.ProductId} not found");

        // 1. Запоминаем файлы и сначала очищаем ссылки в БД
        var fileUrls = product.GetAllFileUrls();
        product.ClearAllFiles();

        await _unitOfWork.SaveChangesAsync(ct);

        // 2. Физические файлы удаляем только после успешного коммита
        foreach (var url in fileUrls)
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(url, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to delete physical file {FileUrl} after DB commit", url);
            }
        }
    }
}
