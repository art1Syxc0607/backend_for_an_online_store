using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Category;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCategoryCommand command, CancellationToken ct)
    {
        var category = await _categoryRepository.GetByIdAsync(command.CategoryId, ct);

        if (category == null) throw new DomainException("No such category");

        await _categoryRepository.DeleteAsync(category, ct);
        await _unitOfWork.SaveChangesAsync();
    }

}
