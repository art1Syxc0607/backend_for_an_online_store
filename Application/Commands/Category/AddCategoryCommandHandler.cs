using Application.Commands.Product;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Category;

internal class AddCategoryCommandHandler : IRequestHandler<AddCategoryCommand, int>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<AddProductCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AddCategoryCommandHandler(ICategoryRepository categoryRepository, ICacheService cacheService,
        ILogger<AddProductCommandHandler> logger, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _cacheService = cacheService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(AddCategoryCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "Category creation started: Name {Name}",
            command.Name
        );

        var category = new Domain.Entities.Category(command.Name, command.Description);

        await _categoryRepository.CreateAsync(category, ct);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Category created successfully: CategoryId {CategoryId}, Name {Name}",
            category.Id,
            category.Name
        );

        // ✅ Очищаем кэш категорий
        await _cacheService.RemoveAsync("categories:all");

        return category.Id;

    }
}
