using FluentValidation;

namespace p48_lessons_asp.Models.ProductRequestModels;

public class CreateProductImageRequest
{
    public Guid ProductId { get; set; }
    public IFormFile? Image { get; set; }
}

public class CreateProductImageValidator : AbstractValidator<CreateProductImageRequest>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    public CreateProductImageValidator()
    {
        RuleFor(img => img.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required");

        RuleFor(img => img.Image)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Image is required")
            .Must(image => image!.Length > 0)
            .WithMessage("Image cannot be empty")
            .Must(image => image!.Length <= 5 * 1024 * 1024)
            .WithMessage("File size must be less than or equal to 5MB")
            .Must(image => AllowedExtensions.Contains(Path.GetExtension(image!.FileName).ToLowerInvariant()))
            .WithMessage("Only JPG, JPEG, PNG and WEBP images are allowed")
            .Must(image => AllowedContentTypes.Contains(image!.ContentType.ToLowerInvariant()))
            .WithMessage("Invalid image content type");
    }
}
