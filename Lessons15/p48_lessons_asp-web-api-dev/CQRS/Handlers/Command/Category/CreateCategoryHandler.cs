using System;
using AutoMapper;
using MediatR;
using p48_lessons_asp.CQRS.Commands.Category;
using p48_lessons_asp.Data;
using p48_lessons_asp.DTO.Category;
using p48_lessons_asp.Services.Slug;

namespace p48_lessons_asp.CQRS.Handlers.Command.Category;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryRequest, CategoryDto?>
{
    private readonly DataContext _db;
    private readonly IMapper _mapper;
    private readonly ISlugService _slugService;
    private readonly ILogger<CreateCategoryHandler> _logger;

    public CreateCategoryHandler(DataContext db, IMapper mapper, ISlugService slugService, ILogger<CreateCategoryHandler> logger)
    {
        _db = db;
        _mapper = mapper;
        _slugService = slugService;
        _logger = logger;
    }
    public async Task<CategoryDto?> Handle(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        Data.Entity.Category category = new()
        {
            Title = request.Title,
            Slug = _slugService.GetSlug(request.Title),
            Description = request.Description
        };
        if (Guid.TryParse(request.ParentCategoryId, out var parsed)) category.ParentCategoryId = parsed;
        try
        {
            _db.Categories.Add(category);
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Succesfully create new category: {id}", category.Id);
            return _mapper.Map<CategoryDto>(category);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Failed create new category");
            return null;
        }
    }
}
