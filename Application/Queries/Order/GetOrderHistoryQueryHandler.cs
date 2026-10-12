using Application.Common;
using Application.DTOs.Order;
using Application.Interfaces;
using AutoMapper;
using Domain.Exceptions;
using MediatR;


namespace Application.Queries.Order;

public class GetOrderHistoryQueryHandler : IRequestHandler
    <GetOrderHistoryQuery, PagedResult<OrderResponseDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetOrderHistoryQueryHandler(
        IOrderRepository orderRepository, IUserRepository userRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<OrderResponseDto>> Handle(
        GetOrderHistoryQuery query, CancellationToken ct)
    {
        // 1. Проверка пользователя
        var user = await _userRepository.GetByIdAsync(query.UserId, ct);
        if (user == null)
            throw new DomainException("User not found.");

        // 2. Нормализация пагинации
        var pageNumber = Math.Max(1, query.PageNumber ?? 1);
        var pageSize = Math.Clamp(query.PageSize ?? 10, 1, 50);

        // 3. Загрузка
        var (items, totalCount) = await _orderRepository
            .GetAllAsync(query, ct);

        // 4. Маппинг
        var dtos = _mapper.Map<List<OrderResponseDto>>(items);

        // ✅ 5. Правильный PagedResult — со ВСЕМИ полями
        return PagedResult<OrderResponseDto>.Create(
            dtos,
            totalCount,
            pageNumber,
            pageSize);
    }
}
