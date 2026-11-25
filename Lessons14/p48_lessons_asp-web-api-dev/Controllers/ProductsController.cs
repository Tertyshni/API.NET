using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using p48_lessons_asp.Data;
using p48_lessons_asp.Data.Entity;
using p48_lessons_asp.DTO.Product;
using p48_lessons_asp.Extensions;
using p48_lessons_asp.Models;
using p48_lessons_asp.Models.ProductRequestModels;
using p48_lessons_asp.Services.Slug;
using p48_lessons_asp.Services.StorageService;

namespace p48_lessons_asp.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IValidator<CreateProductModel> _validator;
        private readonly IValidator<CreateProductImageRequest> _imageValidator;
        private readonly ISlugService _slugService;
        private readonly IStorageService _storageService;
        private readonly IMapper _mapper;

        public ProductsController(DataContext dataContext, IValidator<CreateProductModel> validator, ISlugService slugService, IMapper mapper, IValidator<CreateProductImageRequest> imageValidator, IStorageService storageService)
        {
            _dataContext = dataContext;
            _validator = validator;
            _slugService = slugService;
            _mapper = mapper;
            _imageValidator = imageValidator;
            _storageService = storageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _dataContext.Products.ToListAsync();
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
                System.Console.WriteLine($"[ERROR] Cannot create new product with reason: {ex.Message} | ${DateTime.Now} | ProductsController/CreateProduct");
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
        [Route("product-image")]
        public async Task<IActionResult> CreateProductImage([FromForm]CreateProductImageRequest model)
        {
            var validationResult = _imageValidator.GetErrors(model);
            if (validationResult != null)
            {
                return new ObjectResult(validationResult) { StatusCode = 400 };
            }
            var product = await _dataContext.Products.FirstOrDefaultAsync(p=>p.Id == model.ProductId);
            if(product == null)
            {
                return new ObjectResult(new
                {
                    StatusCode=404,
                    Message="Product not found"
                }) { StatusCode = 404 };
            }
            // todo: transactions

            var imageUrl = await _storageService.SaveFileAsync(nameof(Product), product.Name, model.Image);
            if (string.IsNullOrEmpty(imageUrl))
            {
                return new ObjectResult(new
                {
                    StatusCode=503,
                    Message="Product image not added"
                }) { StatusCode = 503 };
            }
            var productImage = new Data.Entity.ProductImage
            {
                Url = imageUrl,
                ProductId = product.Id,
                Order = 0
            };
            await _dataContext.ProductImages.AddAsync(productImage);
            await _dataContext.SaveChangesAsync();
            return new ObjectResult(new
            {
                StatusCode = 201,
                Message = "Created",
                Data = _mapper.Map<ProductImageDto>(productImage)
            }){StatusCode = 201};
        }

        [HttpDelete("product-image/{id:guid}")]
        public async Task<IActionResult> DeleteProductImage(Guid id)
        {
            var productImage = await _dataContext.ProductImages
                .FirstOrDefaultAsync(image => image.Id == id);

            if (productImage == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Product image not found"
                });
            }

            // Спочатку видаляємо фізичний файл. Якщо це не вдалося,
            // запис у базі даних залишаємо без змін.
            var fileDeleted = await _storageService.DeleteFileAsync(productImage.Url);
            if (!fileDeleted)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    StatusCode = 500,
                    Message = "Could not delete product image from file storage"
                });
            }

            _dataContext.ProductImages.Remove(productImage);

            try
            {
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Файл уже видалено, тому повертаємо помилку і не приховуємо
                // можливу невідповідність між БД та файловою системою.
                throw;
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Product image successfully deleted"
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetNewProduct([FromBody] CreateProductModel model)
        {
           var validationResult = await _validator.GetErrorsAsync(model);
            if (validationResult != null)
            {
                return new ObjectResult(validationResult) { StatusCode = 400 };
            }

            var product = new Data.Entity.Product()
            {
                Name = model.Name,
                Slug = _slugService.GetSlug(model.Name),
                CategoryId = model.CategoryId,
                Price = model.Price,
                Description = model.Description
            };
            try{
            await _dataContext.Products.AddAsync(product);
            await _dataContext.SaveChangesAsync();
            return new ObjectResult(new
            {
                StatusCode = 201,
                Message = "Created",
                Data = _mapper.Map<ProductDto>(product)
            }){StatusCode = 201};
            }
            catch(OperationCanceledException ex)
            {
                System.Console.WriteLine($"Created product error: {ex.Message}");
                return new ObjectResult(new
                {
                    StatusCode = 503,
                    Message = "Service is unavaliable",
                    Data = (object)null!
                })
                { StatusCode = 503 };
            }
        }
        
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest? model)
        {
            return BadRequest();
            // TODO: Validation Logic
            Dictionary<string, string> validationResult = new();
            if (model == null)
            {
                return new ObjectResult(
                    new ResponseModel<object>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Product data is required"
                    })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
            if (model.Name.Trim().Length == 0)
            {
                validationResult["nameError"] = "Product name is required";
            }

            if (model.Price <= 0)
            {
                validationResult["priceError"] = "Product price must be over zero";
            }
            if (validationResult.Count != 0)
            {
                return new ObjectResult(
                   new ResponseModel<object>
                   {
                       StatusCode = StatusCodes.Status400BadRequest,
                       Message = "Product data is required",
                       Data = validationResult
                   })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
            try
            {
                // TODO: Repository Code
                var newProduct = new Data.Entity.Product
                {
                    Name = model.Name,
                    Price = model.Price,
                    Description = model.Description
                };
                _dataContext.Products.Add(newProduct);
                await _dataContext.SaveChangesAsync();
                return new ObjectResult(new ResponseModel<Data.Entity.Product>
                {
                    StatusCode = StatusCodes.Status201Created,
                    Message = "Product successfuly created",
                    Data = newProduct
                });
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
