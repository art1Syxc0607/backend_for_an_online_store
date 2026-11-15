using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Admin.Category;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        ILogger<UpdateCategoryCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCategoryCommand command, CancellationToken ct)
    {
        var category = await _categoryRepository.GetByIdAsync(command.CategoryId, ct);

        if (category == null) throw new DomainException("No such category");

        if (!string.IsNullOrWhiteSpace(command.CategoryName)
            && await _categoryRepository.ExistsByNameAsync(
                command.CategoryName, command.CategoryId, ct))
        {
            throw new DomainException(
                "Категория с таким названием уже существует",
                "DUPLICATE_ENTRY",
                409);
        }

        category.Update(name: command.CategoryName, description: command.CategoryDescription);

        await _categoryRepository.UpdateAsync(category);

        // Optimistic row lock (xmin): if another admin changed this category
        // between our read and this save, EF throws DbUpdateConcurrencyException
        // and the middleware turns it into 409 CONCURRENCY_ERROR.
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            _logger.LogWarning(ex, "Failed to update category {CategoryId}", command.CategoryId);
            throw;
        }
    }
}
