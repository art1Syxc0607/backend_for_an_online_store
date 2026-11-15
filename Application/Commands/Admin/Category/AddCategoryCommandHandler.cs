using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Admin.Category;

internal class AddCategoryCommandHandler : IRequestHandler<AddCategoryCommand, int>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<AddCategoryCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AddCategoryCommandHandler(ICategoryRepository categoryRepository,
        ILogger<AddCategoryCommandHandler> logger, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(AddCategoryCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "Category creation started: Name {Name}",
            command.Name
        );

        // Friendly error for the common case. The unique index on the normalized
        // name is the real guard: it also stops two concurrent inserts, and the
        // resulting 23505 is mapped to 409 by ExceptionHandlingMiddleware.
        if (await _categoryRepository.ExistsByNameAsync(command.Name, ct: ct))
            throw new DomainException(
                "Категория с таким названием уже существует",
                "DUPLICATE_ENTRY",
                409);

        var category = new Domain.Entities.Category(command.Name, command.Description);

        await _categoryRepository.CreateAsync(category, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Category created successfully: CategoryId {CategoryId}, Name {Name}",
            category.Id,
            category.Name
        );

        return category.Id;
    }
}
