using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Favorite;

public class RemoveFromFavoritesCommandHandler : IRequestHandler<RemoveFromFavoritesCommand>
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveFromFavoritesCommandHandler> _logger;

    public RemoveFromFavoritesCommandHandler(
        IFavoriteRepository favoriteRepository,
        IUnitOfWork unitOfWork,
        ILogger<RemoveFromFavoritesCommandHandler> logger)
    {
        _favoriteRepository = favoriteRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(RemoveFromFavoritesCommand command, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            if (!await _favoriteRepository.IsFavoritedAsync(command.UserId, command.ProductId, ct))
            {
                await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                throw new DomainException("Product is not in favorites");
            }

            await _favoriteRepository.RemoveAsync(command.UserId, command.ProductId, ct);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            _logger.LogInformation(
                "Product {ProductId} removed from favorites for user {UserId}",
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
