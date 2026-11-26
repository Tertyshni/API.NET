using FluentValidation;
using Microsoft.EntityFrameworkCore;
using p48_lessons_asp.Data;

namespace p48_lessons_asp.Models.CategoryRequestModels;
public class CreateCategoryModel
{
    public string? ParentCategoryId{get;set;}
    public string Title {get;set;} = string.Empty;
    public string? Description{get;set;}

}

public class CreateCategoryValidator: AbstractValidator<CreateCategoryModel>
{
    private readonly DataContext _dataContext;
    public CreateCategoryValidator(DataContext dataContext)
    {
        _dataContext = dataContext;

        RuleFor(x => x.ParentCategoryId).Must((value) =>
        {
            return Guid.TryParse(value, out var parsedGuid);
        })
        .WithMessage("ParentCategoryId has incorrect format")
        .MustAsync(async (value, token) =>
        {
            Guid.TryParse(value, out Guid parsed);
            var category = await _dataContext.Categories.FirstOrDefaultAsync(c=>c.Id == parsed);
            return category != null;
        }).WithMessage("Category doesn't found")
        .WhenAsync(async(x,_)=>x.ParentCategoryId != null);

        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required")
        .MinimumLength(3).WithMessage("Must have minimum 3 symbols");

        RuleFor(x=> x.Description).MinimumLength(10).WithMessage("Description must have minimum 10 symbols").When(x=>x.Description!=null);
    }
}