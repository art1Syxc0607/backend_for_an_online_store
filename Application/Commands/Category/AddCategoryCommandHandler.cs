using Application.Interfaces;
using MediatR;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Category;

internal class AddCategoryCommandHandler : IRequestHandler<AddCategoryCommand, int>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(AddCategoryCommand command, CancellationToken ct)
    {

        var category = new Domain.Entities.Category(command.Name, command.Description);

        await _categoryRepository.AddAsync(category, ct);
        await _unitOfWork.SaveChangesAsync();

        return category.Id;

    }
}
