using Application.DTOs.Order;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        // 1.
        // Маппинг Order → OrderResponseDto
        CreateMap<Order, OrderResponseDto>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.ShippingAddress))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.PaidAt, opt => opt.MapFrom(src => src.PaidAt))
            .ForMember(dest => dest.ShippedAt, opt => opt.MapFrom(src => src.ShippedAt))
            .ForMember(dest => dest.DeliveredAt, opt => opt.MapFrom(src => src.DeliveredAt))
            .ForMember(dest => dest.ReceivedAt, opt => opt.MapFrom(src => src.ReceivedAt))
            .ForMember(dest => dest.CancelledAt, opt => opt.MapFrom(src => src.CancelledAt));

        // Маппинг OrderItem → OrderItemDto
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.PriceAtPurchase, opt => opt.MapFrom(src => src.PriceAtPurchase))
            .ForMember(dest => dest.ProductNameAtPurchase, opt => opt.MapFrom(src => src.ProductNameAtPurchase));

        // 2.
        // ========== OrderItem → OrderItemDto (туда) ==========
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.PriceAtPurchase, opt => opt.MapFrom(src => src.PriceAtPurchase))
            .ForMember(dest => dest.ProductNameAtPurchase, opt => opt.MapFrom(src => src.ProductNameAtPurchase));

        // ========== OrderItemDto → OrderItem (обратно) ==========
        CreateMap<OrderItemDto, OrderItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Id генерируется БД
            .ForMember(dest => dest.OrderId, opt => opt.Ignore()) // Устанавливается при добавлении
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.PriceAtPurchase, opt => opt.MapFrom(src => src.PriceAtPurchase))
            .ForMember(dest => dest.ProductNameAtPurchase, opt => opt.MapFrom(src => src.ProductNameAtPurchase))
            .ForMember(dest => dest.PurchasePriceAtPurchase, opt => opt.Ignore()) // Не приходит из DTO
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Устанавливается при создании
            .ForMember(dest => dest.Order, opt => opt.Ignore()) // Навигационное свойство
            .ForMember(dest => dest.Product, opt => opt.Ignore()); // Навигационное свойство
    }
}