using Application.Interfaces;
using MediatR;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Category;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCategoryCommand command, CancellationToken ct)
    {
        
        var category = await _categoryRepository.GetByIdAsync(command.CategoryId, ct);

        if (category == null) throw new DomainException("No such category");

        await _categoryRepository.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();
    }
}
