namespace p48_lessons_asp.DTO.Category;
public class CategoryDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ParentCategoryId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // свої властивості, наприклад кількість продуктів, складні поля(які складаються з декількох)
    public int ProductCount{get;set;}
}