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
    public CreateProductValidator(DataContext dataContext)
    {
        RuleFor(m => m.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required")
            .MustAsync(async (categoryId, cancellationToken) =>
                await dataContext.Categories.AnyAsync(
                    c => c.Id == categoryId && c.DeletedAt == null,
                    cancellationToken))
            .WithMessage("Category not found");

        RuleFor(m => m.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Name is required")
            .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Name cannot contain only whitespace")
            .MinimumLength(3).WithMessage("Name must have at least 3 characters")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(m => m.Description)
            .MinimumLength(10).WithMessage("Description must have at least 10 characters")
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(m => !string.IsNullOrWhiteSpace(m.Description));

        RuleFor(m => m.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero");
    }
}
