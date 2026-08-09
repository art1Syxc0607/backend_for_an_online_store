using Application.Commands.Category;
using Application.DTOs.Category;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Controllers.Admin;

[Route("api/admin/category")]
[Authorize(Roles = "Admin")]
public class AdminCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileStorageService _fileStorageService;

    public AdminCategoriesController(IMediator mediator, IFileStorageService fileStorageService)
    {
        _mediator = mediator;
        _fileStorageService = fileStorageService;
    }

    [HttpPost]
    public async Task<ActionResult<int>> AddCategory([FromBody] AddCategoryDto dto)
    {
        var command = new AddCategoryCommand
        {
            Name = dto.Name,
            Description = dto.Description,
        };

        return await _mediator.Send(command);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryDto dto)
    {
        var command = new UpdateCategoryCommand
        {
            CategoryDescription = dto.Description,
            CategoryName = dto.Name
        };

        await _mediator.Send(command);

        return Ok();
    }

    [HttpDelete("{catedoryId}")]
    public async Task<IActionResult> DeleteCategory(int catedoryId)
    {
        var command = new DeleteCategoryCommand
        {
            CategoryId = catedoryId
        };

        await _mediator.Send(command);

        return Ok();
    }
}
