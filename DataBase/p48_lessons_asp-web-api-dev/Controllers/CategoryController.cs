using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using p48_lessons_asp.Data;
using p48_lessons_asp.Data.Entity;
using p48_lessons_asp.DTO.Category;
using p48_lessons_asp.Models.CategoryRequestModels;
using p48_lessons_asp.Services.FluentValidation;
using p48_lessons_asp.Services.Slug;

namespace p48_lessons_asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ISlugService _slugService;
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;
        IValidator<CreateCategoryModel> _createCategoryValidator;
        public CategoryController(
            IMapper mapper,
            ISlugService slugService,
            DataContext dataContext,
            IValidator<CreateCategoryModel> createCategoryValidator
            )
        {
            _slugService = slugService;
            _dataContext = dataContext;
            _createCategoryValidator = createCategoryValidator;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                // var categories = await _dataContext.Categories.Where(c => c.DeletedAt == null).Select(c => new
                // {
                //     c.Id,
                //     ParentCategory = c.ParentCategoryId,
                //     Slug = c.Slug,
                //     Title = c.Title,
                //     Description = c.Description
                // }).ToListAsync();
                // var categories = await _dataContext.Categories.Where(c => c.DeletedAt == null).Select(c => new CategoryDto()).ToListAsync();
                var categories = await _dataContext
                .Categories
                .Where(c => c.DeletedAt == null)
                // підванатажуємо продукти, самостійно пов'язані сутності EF не підтягує
                .Include(c=>c.Products)
                .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

                if (categories == null || categories.Count == 0)
                {
                    return new ObjectResult(new
                    {
                        StatusCode = 404,
                        Message = "Categories not found",
                        Data = (object)null!
                    })
                    { StatusCode = 404 };
                }
                return new ObjectResult(new
                {
                    StatusCode = 200,
                    Message = "Ok",
                    Count = categories.Count,
                    Data = categories
                })
                { StatusCode = 200 };
            }
            catch (OperationCanceledException ex)
            {
                System.Console.WriteLine("Database error: {0}", ex.Message);
                return new ObjectResult(new
                {
                    StatusCode = 503,
                    Message = "Service Unavailable",
                    Data = (object)null!
                })
                { StatusCode = 503 };
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryModel model)
        {
            var validations = await _createCategoryValidator.GetErrorsAsync(model);

            if (validations != null)
            {
                return new ObjectResult(validations) { StatusCode = 400 };
            }

            Category category = new()
            {
                Title = model.Title,
                Slug = _slugService.GetSlug(model.Title),
                Description = model.Description
            };
            if (Guid.TryParse(model.ParentCategoryId, out var parsed)) category.ParentCategoryId = parsed;
            try
            {
                await _dataContext.Categories.AddAsync(category);
                await _dataContext.SaveChangesAsync();
                return new ObjectResult(new
                {
                    StatusCdoe = 201,
                    Message = "Created",
                    Data = category
                });
            }
            catch (OperationCanceledException ex)
            {
                System.Console.WriteLine($"Created category error: {ex.Message}");
                return new ObjectResult(new
                {
                    StatusCode = 500,
                    Message = "See server logs",
                    Data = (object)null!
                })
                { StatusCode = 500 };
            }
        }
    }
}
