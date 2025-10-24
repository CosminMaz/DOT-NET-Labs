using AutoMapper;
using System;

namespace OrderManagementAPI.Features.Order
{
    public class AdvancedOrderMappingProfile : Profile
    {
        public AdvancedOrderMappingProfile()
        {
            CreateMap<CreateOrderProfileRequest, Order>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.StockQuantity > 0))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CoverImageUrl, opt => opt.MapFrom(src => src.Category == OrderCategory.Children ? null : src.CoverImageUrl))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Category == OrderCategory.Children ? src.Price * 0.9m : src.Price));
        }
    }
}
