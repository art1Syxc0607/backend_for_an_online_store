using Application.DTOs.Order;
using Application.DTOs.Product;
using AutoMapper;
using Domain.DTOs.Order;
using Domain.Entities;


namespace Application.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductResponseDto>()
            .ForMember(dest => dest.AverageRating,
                opt => opt.MapFrom(src => src.GetAverageRating()))
            .ForMember(dest => dest.CountOfReviews,
                opt => opt.MapFrom(src => src.Reviews.Count))
            .ForMember(dest => dest.AmountOfRecieved,
                opt => opt.MapFrom(src => src.AmountOfReceived))
            .ForMember(dest => dest.AmountOfPaid,
                opt => opt.MapFrom(src => src.AmountOfPaid))
            .ForMember(dest => dest.AmountOfCanceled,
                opt => opt.MapFrom(src => src.AmountOfCanceled))
            .ForMember(dest => dest.CountOfOrdersContainThisProduct,
                opt => opt.MapFrom(src => src.OrderItems.Count()))
            .ForMember(dest => dest.ImageUrls,
                opt => opt.MapFrom(src => src.ImageUrls.ToList()))
            .ForMember(dest => dest.VideoUrls,
                opt => opt.MapFrom(src => src.VideoUrls.ToList()));
    }
}
