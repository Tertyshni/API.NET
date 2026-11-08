using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using p48_lessons_asp.Data;
using p48_lessons_asp.Models;

namespace p48_lessons_asp.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
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

        
    }

}
