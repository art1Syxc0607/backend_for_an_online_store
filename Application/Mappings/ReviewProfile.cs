using AutoMapper;
using Domain.Entities;
using Application.DTOs.Review;

namespace Application.Mappings;

public class ReviewProfile : Profile
{
    public ReviewProfile()
    {
        CreateMap<Review, ReviewResponseDto>()
            // Явное указание для всех полей
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Unknown"))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : "Unknown"))

            .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Text))
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating))
            .ForMember(dest => dest.ImagesUrls, opt => opt.MapFrom(src =>
                src.ImageUrls))
            .ForMember(dest => dest.VideosUrls, opt => opt.MapFrom(src =>
                src.VideoUrls))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.AdminResponse, opt => opt.MapFrom(src => src.AdminResponse))
            .ForMember(dest => dest.AdminResponseAt, opt => opt.MapFrom(src => src.AdminResponseAt));
    }

}
