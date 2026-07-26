using Application.Commands.Category;
using Application.Queries.Category;
using Application.DTOs.Category;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;


[Route("api/category")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoryController(IMediator mediator)
        => _mediator = mediator;

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryResponseDto>> GetCategoryById(int id)
    {
        var command = new GetCategoryCommand
        {
            Id = id
        };

        return await _mediator.Send(command);
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryResponseDto>>> GetAllCategories()
    {
        var command = new GetAllCategoriesQueries();

        return await _mediator.Send(command);
    }

    [Authorize(Roles = "admin")]
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

    [Authorize(Roles = "admin")]
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

    [Authorize(Roles = "admin")]
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
