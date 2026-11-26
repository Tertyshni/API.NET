using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using p48_lessons_asp.CQRS.Commands.Category;
using p48_lessons_asp.CQRS.Queries.CategoryRequests;
using p48_lessons_asp.Data;
using p48_lessons_asp.Data.Entity;
using p48_lessons_asp.DTO.Category;
using p48_lessons_asp.Extensions;
using p48_lessons_asp.Services.Slug;

namespace p48_lessons_asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CategoryController> _logger;
        private readonly ISlugService _slugService;
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;
        IValidator<CreateCategoryRequest> _createCategoryValidator;
        IValidator<GetCategoryByIdRequest> _getCategoryByIdValidator;
        public CategoryController(
            IMapper mapper,
            ISlugService slugService,
            DataContext dataContext,
            IValidator<CreateCategoryRequest> createCategoryValidator,
            ILogger<CategoryController> logger,
            IValidator<GetCategoryByIdRequest> getCategoryByIdValidator,
            IMediator mediator)
        {
            _slugService = slugService;
            _dataContext = dataContext;
            _createCategoryValidator = createCategoryValidator;
            _mapper = mapper;
            _logger = logger;
            _getCategoryByIdValidator = getCategoryByIdValidator;
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize]
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
                .Include(c => c.Products)
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
                _logger.LogError(ex, "Database error");
                return new ObjectResult(new
                {
                    StatusCode = 503,
                    Message = "Service Unavailable",
                    Data = (object)null!
                })
                { StatusCode = 503 };
            }
        }


        [HttpGet]
        [Route("one")]
        public async Task<IActionResult> GetCategory([FromQuery] GetCategoryByIdRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new { StatusCode = 400, Message = "Category data is required" });

            var validationResult = await _getCategoryByIdValidator.GetErrorsAsync(request);
            if (validationResult != null)
            {
                return new ObjectResult(new
                {
                    StatusCode = 400,
                    Message = "Bad Request",
                    Errors = validationResult
                })
                { StatusCode = 400 };
            }
            var category = await _mediator.Send(request, cancellationToken);
            if (category == null) return new ObjectResult(new
            {
                StatusCode = 404,
                Message = "Not Found",
            })
            { StatusCode = 404 };

            return new ObjectResult(new
            {
                StatusCode = 200,
                Message = "Ok",
                Data = category
            })
            { StatusCode = 200 };
        }
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            if (request == null)
                return BadRequest(new { StatusCode = 400, Message = "Category data is required" });

            var validations = await _createCategoryValidator.GetErrorsAsync(request);

            if (validations != null)
            {
                _logger.LogWarning("User failed validation while create new category");
                return new ObjectResult(validations) { StatusCode = 400 };
            }

            var newCategory = await _mediator.Send(request);
            if(newCategory==null) return new ObjectResult(new
            {
                StatusCode = 503,
                Message="Database service unavaliable",
            }){StatusCode = 503};
            return new ObjectResult(new
            {
                StatusCode = 201,
                Message = "Ok",
                Data = newCategory
            }){StatusCode = 201};
        }
    }
}
