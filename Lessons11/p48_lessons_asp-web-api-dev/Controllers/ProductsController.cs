using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using p48_lessons_asp.Data;
using p48_lessons_asp.Data.Entity;
using p48_lessons_asp.DTO.Product;
using p48_lessons_asp.Models;
using p48_lessons_asp.Models.ProductRequestModels;
using p48_lessons_asp.Services.FluentValidation;
using p48_lessons_asp.Services.Slug;

namespace p48_lessons_asp.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IValidator<CreateProductModel> _validator;
        private readonly ISlugService _slugService;
        private readonly IMapper _mapper;

        public ProductsController(DataContext dataContext, IValidator<CreateProductModel> validator, ISlugService slugService, IMapper mapper)
        {
            _dataContext = dataContext;
            _validator = validator;
            _slugService = slugService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _dataContext.Products.Include(p => p.Category).ToListAsync();
                if(products == null || products.Count == 0)
                {
                    return new ObjectResult(
                        new ResponseModel<object>
                        {
                            StatusCode = 404,
                            Message = "Products not found",
                        }
                    )
                    {
                        StatusCode = 404
                    };
                }
                return new ObjectResult(
                    new ResponseModel<List<Data.Entity.Product>>
                    {
                        StatusCode = 200,
                        Message = "Ok",
                        Data = products
                    }
                ){StatusCode = 200};
            }
            catch (OperationCanceledException ex)
            {
                // TODO: Logger
                Console.ForegroundColor = ConsoleColor.DarkRed;
                System.Console.WriteLine($"[CRITICAL] Cannot create new product with reason: {ex.Message} | ${DateTime.Now} | ProductsController/CreateProduct");
                Console.ResetColor();
                

                return new ObjectResult(
                   new ResponseModel<object>
                   {
                       StatusCode = StatusCodes.Status500InternalServerError,
                       Message = "Cannot create product. See server logs",
                   })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductModel? model)
        {
            if (model is null)
            {
                return BadRequest(new ResponseModel<object>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Product data is required"
                });
            }

            var validationErrors = await _validator.GetErrorsAsync(model);
            if (validationErrors != null)
            {
                return BadRequest(new ResponseModel<object>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Validation failed",
                    Data = validationErrors
                });
            }

            var product = new Data.Entity.Product
            {
                Name = model.Name.Trim(),
                Slug = _slugService.GetSlug(model.Name.Trim()),
                CategoryId = model.CategoryId,
                Price = model.Price,
                Description = string.IsNullOrWhiteSpace(model.Description)
                    ? null
                    : model.Description.Trim()
            };

            try
            {
                await _dataContext.Products.AddAsync(product);
                await _dataContext.SaveChangesAsync();

                return StatusCode(StatusCodes.Status201Created, new ResponseModel<ProductDto>
                {
                    StatusCode = StatusCodes.Status201Created,
                    Message = "Product successfully created",
                    Data = _mapper.Map<ProductDto>(product)
                });
            }
            catch (DbUpdateException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel<object>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "Cannot create product"
                });
            }
        }

        #region Old_example_without_db 
        /*
        private readonly IGenerator<Product> _generator;
        private readonly List<Product> products;
        public ProductsController(IGenerator<Product> generator)
        {
            _generator = generator;
            products = _generator.Generate(100);

        }

        [HttpGet]
        public IActionResult Products([FromQuery] GetProductQueryModel? model)
        {
            System.Console.WriteLine(model);
            return Ok(new
            {
                Status = 200,
                Message = "Ok",
                Products = products
            });
        }
        [HttpGet]
        [Route("query")]
        public IActionResult GetProductsIn([FromQuery] int? startFrom, [FromQuery] int? endsWith)
        {
            if (startFrom.HasValue)
            {
                if (startFrom.Value < 0 || startFrom.Value > products.Count - 1)
                {
                    return BadRequest(new
                    {
                        Status = 400,
                        Message = $"Start from must be in 0-{products.Count}"
                    });
                }
                return Ok(products.Slice(startFrom.Value, (products.Count - 1) - startFrom.Value));
            }
            if (endsWith.HasValue)
            {
                if (endsWith < products.Count || endsWith > products.Count - 1)
                {
                    return BadRequest(new
                    {
                        Status = 400,
                        Message = $"endsWith must be in 0-{products.Count - 1}"
                    });
                }
            }
            return NotFound();
        }

        //Реалізація рут параметру
        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            if (id > (products.Count - 1)) return NotFound(new
            {
                Status = 404,
                Message = $"Product with id-{id} not found",
                Data = (object)null!
            });
            // return Ok(products[id]);
            return Ok(new
            {
                Status = 200,
                Message = "Ok",
                Data = products[id]
            });
        }
        // POST: api/products/

        // [FromBody] Only Json body
        // [FromForm] Використовується для форм з html, + надає можливість отримувати файли від користувача
        
        // public IActionResult CreateProducts([FromBody] CreateProductModel? model)
        [HttpPost]
        public async Task<IActionResult> CreateProducts([FromForm] CreateProductModel? model)
        {
            if (model is null)
            {
                // return BadRequest();
                return new ObjectResult(new { message = "Product body is required" }) { StatusCode = StatusCodes.Status400BadRequest };
            }
            if (model.productImage != null)
            {
                var file = model.productImage;
                System.Console.WriteLine("File name: {0}", file.FileName);
                // System.Console.WriteLine("Content: {0}", file.ToString());
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                using(var stream = file.OpenReadStream())
                using(var reader = new StreamReader(stream))
                {
                    string content = reader.ReadToEnd();
                    System.Console.WriteLine(content);
                }
                Console.ResetColor();
            }
            System.Console.WriteLine(model.Product);
            return new ObjectResult(new { status = 201, message = "Created", data = model.Product }) { StatusCode = StatusCodes.Status201Created };
        }

        */
        #endregion
    }

}
