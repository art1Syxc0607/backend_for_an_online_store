using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Favorite;

public class AddToFavoritesCommandHandler : IRequestHandler<AddToFavoritesCommand>
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddToFavoritesCommandHandler> _logger;

    public AddToFavoritesCommandHandler(
        IFavoriteRepository favoriteRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<AddToFavoritesCommandHandler> logger)
    {
        _favoriteRepository = favoriteRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(AddToFavoritesCommand command, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // Проверяем, что продукт существует
            var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
            if (product == null)
            {
                await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                throw new DomainException("Product not found", "NOT_FOUND", 404);
            }

            // Проверяем, не добавлен ли уже товар в избранное
            if (await _favoriteRepository.IsFavoritedAsync(command.UserId, command.ProductId, ct))
            {
                await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                throw new DomainException(
                    "Product is already in favorites",
                    "DUPLICATE_ENTRY",
                    409);
            }

            var favorite = new Domain.Entities.Favorite(command.UserId, command.ProductId);
            await _favoriteRepository.AddAsync(favorite, ct);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            _logger.LogInformation(
                "Product {ProductId} added to favorites for user {UserId}",
                command.ProductId,
                command.UserId);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }
}
