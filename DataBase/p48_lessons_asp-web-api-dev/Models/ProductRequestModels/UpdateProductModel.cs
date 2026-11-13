namespace p48_lessons_asp.Models.ProductRequestModels;

public class UpdateProductModel
{
    public Guid? CategoryId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
}
