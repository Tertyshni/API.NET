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

        CreateMap<Data.Entity.Product, ProductDto>()
        .ForMember(dest => dest.CategoryName,
            opt => opt.MapFrom(src => src.Category != null ? src.Category.Title : string.Empty))
        .ForMember(dest => dest.ImageUrl,
            opt => opt.MapFrom(src => src.ProductImages == null
                ? null
                : src.ProductImages
                    .OrderBy(image => image.Order)
                    .ThenBy(image => image.CreatedAt)
                    .Select(image => image.Url)
                    .FirstOrDefault()));
        CreateMap<Data.Entity.ProductImage, ProductImageDto>();

        CreateMap<Data.Entity.User, UserDto>();
    }
}