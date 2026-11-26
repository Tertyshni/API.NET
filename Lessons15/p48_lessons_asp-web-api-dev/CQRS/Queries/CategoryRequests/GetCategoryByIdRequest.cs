using System;
using FluentValidation;
using MediatR;
using p48_lessons_asp.DTO.Category;

namespace p48_lessons_asp.CQRS.Queries.CategoryRequests;

public class GetCategoryByIdRequest: IRequest<CategoryDto?>
{
    public string CategoryId{get;set;} = string.Empty;
}

public class GetCategoryByIdValidator: AbstractValidator<GetCategoryByIdRequest>
{
    public GetCategoryByIdValidator()
    {
        RuleFor(r=>r.CategoryId).NotEmpty().WithMessage("CategoryId is required");
        RuleFor(r=>r.CategoryId)
        .Must(x=> Guid.TryParse(x, out var _) == true)
        .WithMessage("Incorrect {PropertyName} format")
        .When(x => !string.IsNullOrEmpty(x.CategoryId));
    }
}
