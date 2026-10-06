using Application.Common;
using Application.DTOs.Product;
using Application.Enums;
using Application.Interfaces;
using AutoMapper;
using Domain.Exceptions;
using MediatR;
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
    private readonly IMapper _mapper;

    public GetProductsFilterCommandHandler(IProductRepository productRepository, 
        IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductResponseDto>> Handle(
        GetProductsFilterCommand command,
        CancellationToken ct)
    {
        var pageNumber = Math.Max(1, command.PageNumber ?? 1);
        var pageSize = Math.Clamp(command.PageSize ?? 20, 1, 50);

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
