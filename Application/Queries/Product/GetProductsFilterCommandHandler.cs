using Application.Common;
using Application.DTOs.Product;
using Application.Enums;
using Application.Interfaces;
using AutoMapper;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Product;

public class GetProductsFilterCommandHandler : IRequestHandler<GetProductsFilterCommand,
    PagedResult<ProductResponseDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetProductsFilterCommandHandler> _logger;
    private readonly IMapper _mapper;

    public GetProductsFilterCommandHandler(IProductRepository productRepository, 
        ILogger<GetProductsFilterCommandHandler> logger,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductResponseDto>> Handle(
        GetProductsFilterCommand command,
        CancellationToken ct)
    {
        var pageNumber = Math.Max(1, command.PageNumber ?? 1);
        var pageSize = Math.Clamp(command.PageSize ?? 20, 1, 50);

        _logger.LogInformation("SearchText received: '{SearchText}', Length: {Length}",
    command.SearchText ?? "NULL", command.SearchText?.Length ?? 0);

        // Репозиторий уже умеет фильтрацию
        var (items, totalCount) = await _productRepository.GetProductsFilter(
            command, ct);

        return PagedResult<ProductResponseDto>.Create(
            _mapper.Map<List<ProductResponseDto>>(items),
            totalCount,
            pageNumber,
            pageSize);
    }
}
