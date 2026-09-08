using Application.DTOs.Review;
using Application.Enums;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.Review;

public class GetAllReviewsHandler : IRequestHandler<GetAllReviewsCommand, List<ReviewResponseDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllReviewsHandler> _logger;

    public GetAllReviewsHandler(
        IReviewRepository reviewRepository, IMapper mapper,
        ILogger<GetAllReviewsHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<ReviewResponseDto>> Handle(
        GetAllReviewsCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation("Getting all reviews with filters: {@Filters}", command);

        // 1. Получаем все отзывы с фильтрацией
        var reviews = await _reviewRepository.GetAllWithFiltersAsync(
            command,
            ct
        );

        // 2. Формируем DTO
        // AutoMapper автоматически применит все настройки
        var reviewDtos = _mapper.Map<List<ReviewResponseDto>>(reviews);

        _logger.LogInformation("Mapping into dto completed successfully. Count: {Count}"
            , reviewDtos.Count);

        return reviewDtos;
    }
}
