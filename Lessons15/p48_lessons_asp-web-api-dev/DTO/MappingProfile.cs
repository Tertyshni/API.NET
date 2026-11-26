using AutoMapper;
using p48_lessons_asp.DTO.Category;
using p48_lessons_asp.DTO.Product;
using p48_lessons_asp.DTO.User;

namespace p48_lessons_asp.DTO;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Data.Entity.Category, CategoryDto>()
        .ForMember(dest => dest.ProductCount,
        opt =>
            opt.MapFrom(src => src.Products != null ? src.Products.Count() : 0));

        CreateMap<Data.Entity.Product, ProductDto>();
        CreateMap<Data.Entity.ProductImage, ProductImageDto>();

        CreateMap<Data.Entity.User, UserDto>();
    }
}