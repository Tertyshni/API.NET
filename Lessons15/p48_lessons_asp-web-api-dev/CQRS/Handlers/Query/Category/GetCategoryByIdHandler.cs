using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using p48_lessons_asp.CQRS.Queries.CategoryRequests;
using p48_lessons_asp.Data;
using p48_lessons_asp.DTO.Category;

namespace p48_lessons_asp.CQRS.Handlers.Query.Category;


// Виконавець команд, для кожного запиту/команди створюється свій виконавець
public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdRequest, CategoryDto?>
{
    private readonly DataContext _db;
    private readonly IMapper _mapper;

    public GetCategoryByIdHandler(DataContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<CategoryDto?> Handle(GetCategoryByIdRequest request, CancellationToken cancellationToken)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c=>c.Id == Guid.Parse(request.CategoryId), cancellationToken);
        if(category == null) return null;
        return _mapper.Map<CategoryDto>(category);
    }
}
