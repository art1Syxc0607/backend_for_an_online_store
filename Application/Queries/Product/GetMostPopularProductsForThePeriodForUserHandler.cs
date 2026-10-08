// Application/Commands/Product/GetMostPopularProductsForThePeriodHandler.cs
using Application.Common;
using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Product;

public class GetMostPopularProductsForThePeriodForUserHandler
    : IRequestHandler<GetMostPopularProductsForThePeriodForUserCommand, PagedResult<PopularProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetMostPopularProductsForThePeriodForUserHandler> _logger;

    public GetMostPopularProductsForThePeriodForUserHandler(
        IProductRepository productRepository,
        ILogger<GetMostPopularProductsForThePeriodForUserHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<PagedResult<PopularProductDto>> Handle(
        GetMostPopularProductsForThePeriodForUserCommand command,
        CancellationToken ct = default)
    {
        if (command.FirstDayOfThePeriod > command.LastDayOfThePeriod)
        {
            _logger.LogWarning(
                "FirstDate {FirstDate} is later than LastDate {LastDate}",
                command.FirstDayOfThePeriod,
                command.LastDayOfThePeriod);

            throw new DomainException("FirstDate can't be later than LastDate");
        }

        var pageNumber = Math.Max(1, command.PageNumber ?? 1);
        var pageSize = Math.Clamp(command.PageSize ?? 20, 1, 50);

        _logger.LogInformation(
            "Getting popular products: {From} - {To}, page {Page}, size {Size}",
            command.FirstDayOfThePeriod,
            command.LastDayOfThePeriod,
            pageNumber,
            pageSize);

        return await _productRepository.GetMostPopularProductsForThePeriodForUserAsync(
            command,
            ct);
    }
}