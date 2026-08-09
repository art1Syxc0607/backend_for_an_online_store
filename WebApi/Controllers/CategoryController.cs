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

}
