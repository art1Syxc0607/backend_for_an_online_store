using Application.Commands.Favorite;
using Application.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApplicationTests.Favorites;

public class RemoveFromFavoritesHandlerTests
{
    [Fact]
    public async Task Handle_WhenFavoriteRemoved_ShouldCommit()
    {
        // Arrange
        var favoriteRepository = new Mock<IFavoriteRepository>();
        favoriteRepository
            .Setup(x => x.RemoveAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new RemoveFromFavoritesCommandHandler(
            favoriteRepository.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<RemoveFromFavoritesCommandHandler>>());

        // Act
        await handler.Handle(
            new RemoveFromFavoritesCommand { UserId = 1, ProductId = 10 },
            CancellationToken.None);

        // Assert
        unitOfWork.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenFavoriteAlreadyRemoved_ShouldBeIdempotent()
    {
        // Arrange: a parallel request deleted the row first, so 0 rows are affected.
        var favoriteRepository = new Mock<IFavoriteRepository>();
        favoriteRepository
            .Setup(x => x.RemoveAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new RemoveFromFavoritesCommandHandler(
            favoriteRepository.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<RemoveFromFavoritesCommandHandler>>());

        // Act
        Func<Task> act = async () => await handler.Handle(
            new RemoveFromFavoritesCommand { UserId = 1, ProductId = 10 },
            CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        unitOfWork.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldRollback()
    {
        // Arrange
        var favoriteRepository = new Mock<IFavoriteRepository>();
        favoriteRepository
            .Setup(x => x.RemoveAsync(1, 10, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db failure"));

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new RemoveFromFavoritesCommandHandler(
            favoriteRepository.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<RemoveFromFavoritesCommandHandler>>());

        // Act
        Func<Task> act = async () => await handler.Handle(
            new RemoveFromFavoritesCommand { UserId = 1, ProductId = 10 },
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        unitOfWork.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
