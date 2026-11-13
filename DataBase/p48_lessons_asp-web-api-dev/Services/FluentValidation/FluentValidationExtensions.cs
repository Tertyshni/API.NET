using FluentValidation;

namespace p48_lessons_asp.Services.FluentValidation;

public static class FluentValidationExtensions
{
    public static List<object> GetErrors<T>(this IValidator<T> validator, T instance)
    {
        var res = validator.Validate(instance);
        if (!res.IsValid)
        {
            return res.Errors.Select(e=> new
            {
                propertyName = e.PropertyName,
                errorMessage=e.ErrorMessage
            }).ToList<object>();
        }
        return null;
    }
    public static async Task<List<object>> GetErrorsAsync<T>(this IValidator<T> validator, T instance)
    {
        var res = await validator.ValidateAsync(instance);
        if (!res.IsValid)
        {
            return res.Errors.Select(e=> new
            {
                propertyName = e.PropertyName,
                errorMessage=e.ErrorMessage
            }).ToList<object>();
        }
        return null;
    }
}