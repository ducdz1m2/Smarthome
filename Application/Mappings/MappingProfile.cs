

using Application.DTOs.Responses;
using AutoMapper;
using Domain.Entities.Catalog;
using System.Xml.Serialization;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductResponse>()
                .ForMember(dest => dest.Sku, opt => opt.MapFrom(src => src.Sku.Value))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.BasePrice.Amount))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name));
        }
    }
}
