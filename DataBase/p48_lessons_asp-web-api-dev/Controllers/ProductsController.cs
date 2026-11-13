using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using p48_lessons_asp.Data;
using p48_lessons_asp.DTO.Product;
using p48_lessons_asp.Models.ProductRequestModels;
using p48_lessons_asp.Services.FluentValidation;
using p48_lessons_asp.Services.Slug;

namespace p48_lessons_asp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly DataContext _dataContext;
    private readonly IValidator<CreateProductModel> _validator;
    private readonly ISlugService _slugService;
    private readonly IMapper _mapper;

    public ProductsController(
        DataContext dataContext,
        IValidator<CreateProductModel> validator,
        ISlugService slugService,
        IMapper mapper)
    {
        _dataContext = dataContext;
        _validator = validator;
        _slugService = slugService;
        _mapper = mapper;
    }

    // Повертає тільки товари, які не видалені.
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _dataContext.Products
            .Where(p => p.DeletedAt == null)
            .Include(p => p.Category)
            .ToListAsync();

        return Ok(new
        {
            StatusCode = 200,
            Message = "Ok",
            Data = _mapper.Map<List<ProductDto>>(products)
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var product = await _dataContext.Products
            .Where(p => p.DeletedAt == null)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound(new { StatusCode = 404, Message = "Product not found" });

        return Ok(new
        {
            StatusCode = 200,
            Message = "Ok",
            Data = _mapper.Map<ProductDto>(product)
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductModel model)
    {
        var validationResult = await _validator.GetErrorsAsync(model);
        if (validationResult != null)
            return BadRequest(validationResult);

        var product = new Data.Entity.Product
        {
            Name = model.Name,
            Slug = _slugService.GetSlug(model.Name),
            CategoryId = model.CategoryId,
            Price = model.Price,
            Description = model.Description
        };

        _dataContext.Products.Add(product);
        await _dataContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new
        {
            StatusCode = 201,
            Message = "Created",
            Data = _mapper.Map<ProductDto>(product)
        });
    }

    // Повне редагування: клієнт надсилає всі поля товару.
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] CreateProductModel model)
    {
        var product = await _dataContext.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null);

        if (product == null)
            return NotFound(new { StatusCode = 404, Message = "Product not found" });

        var validationResult = await _validator.GetErrorsAsync(model);
        if (validationResult != null)
            return BadRequest(validationResult);

        product.Name = model.Name;
        product.Slug = _slugService.GetSlug(model.Name);
        product.CategoryId = model.CategoryId;
        product.Price = model.Price;
        product.Description = model.Description;

        await _dataContext.SaveChangesAsync();

        return Ok(new
        {
            StatusCode = 200,
            Message = "Product updated",
            Data = _mapper.Map<ProductDto>(product)
        });
    }

    // Часткове редагування: змінюються тільки передані поля.
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> PartiallyUpdateProduct(Guid id, [FromBody] UpdateProductModel model)
    {
        var product = await _dataContext.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null);

        if (product == null)
            return NotFound(new { StatusCode = 404, Message = "Product not found" });

        if (model.Name != null)
        {
            if (model.Name.Trim().Length < 3)
                return BadRequest(new { StatusCode = 400, Message = "Name must have minimum 3 symbols" });

            product.Name = model.Name;
            product.Slug = _slugService.GetSlug(model.Name);
        }

        if (model.CategoryId.HasValue)
        {
            var categoryExists = await _dataContext.Categories
                .AnyAsync(c => c.Id == model.CategoryId.Value);

            if (!categoryExists)
                return BadRequest(new { StatusCode = 400, Message = "Category not found" });

            product.CategoryId = model.CategoryId.Value;
        }

        if (model.Description != null)
        {
            if (model.Description.Length < 10)
                return BadRequest(new { StatusCode = 400, Message = "Description must have minimum 10 symbols" });

            product.Description = model.Description;
        }

        if (model.Price.HasValue)
        {
            if (model.Price.Value < 0)
                return BadRequest(new { StatusCode = 400, Message = "Price cannot be less than zero" });

            product.Price = model.Price.Value;
        }

        await _dataContext.SaveChangesAsync();

        return Ok(new
        {
            StatusCode = 200,
            Message = "Product partially updated",
            Data = _mapper.Map<ProductDto>(product)
        });
    }

    // М'яке видалення: запис залишається в базі, але отримати його через GET не можна.
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> SoftDeleteProduct(Guid id)
    {
        var product = await _dataContext.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null);

        if (product == null)
            return NotFound(new { StatusCode = 404, Message = "Product not found" });

        product.DeletedAt = DateTime.UtcNow;
        await _dataContext.SaveChangesAsync();

        return Ok(new { StatusCode = 200, Message = "Product soft-deleted" });
    }

    // Повне видалення: запис фізично видаляється з бази даних.
    [HttpDelete("{id:guid}/hard")]
    public async Task<IActionResult> HardDeleteProduct(Guid id)
    {
        var product = await _dataContext.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound(new { StatusCode = 404, Message = "Product not found" });

        _dataContext.Products.Remove(product);
        await _dataContext.SaveChangesAsync();

        return Ok(new { StatusCode = 200, Message = "Product permanently deleted" });
    }
}
