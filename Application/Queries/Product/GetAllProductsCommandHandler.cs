using Application.DTOs.Product;
using Application.Interfaces;
using MediatR;
using AutoMapper;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Exceptions;
using Application.Common;

namespace Application.Queries.Product;

public class GetAllProductsHandler
    : IRequestHandler<GetAllProductsCommand, PagedResult<ProductResponseDto>>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public GetAllProductsHandler(
        IProductRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductResponseDto>> Handle(
        GetAllProductsCommand command,
        CancellationToken ct)
    {
        // ✅ Нормализация
        var pageNumber = Math.Max(1, command.PageNumber);
        var pageSize = Math.Clamp(command.PageSize, 1, 50);

        // ✅ Репозиторий возвращает (items, totalCount)
        var (items, totalCount) = await _repository.GetAllProductsAsync(
            pageNumber, pageSize, ct);

        // ✅ Маппинг
        var dtos = _mapper.Map<List<ProductResponseDto>>(items);

        return PagedResult<ProductResponseDto>.Create(
            dtos,
            totalCount,
            pageNumber,
            pageSize);
    }
}
