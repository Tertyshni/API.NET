using System;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using p48_lessons_asp.Data;

namespace p48_lessons_asp.Models.ProductRequestModels;

public class CreateProductModel
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
}

public class CreateProductValidator : AbstractValidator<CreateProductModel>
{
    private readonly DataContext _dataContext;
    public CreateProductValidator(DataContext dataContext)
    {
        _dataContext = dataContext;

        RuleFor(m => m.CategoryId)
        .NotEmpty().WithMessage("CategoryId is required")
        .MustAsync(async (categoryId, token) =>
        {
            var category = await _dataContext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            return category != null;
        }).WithMessage("Category not found");

        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required")
        .MinimumLength(3).WithMessage("Must have minimum 3 symbols");

        RuleFor(x => x.Description).MinimumLength(10)
        .WithMessage("Description must have minimum 10 symbols")
        .When(x => x.Description != null);

        RuleFor(x=>x.Price).NotEmpty().WithMessage("Price is required")
        .GreaterThanOrEqualTo(0.0m).WithMessage("Price cant be less then zero");
    }
}
