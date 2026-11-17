namespace p48_lessons_asp.Data.Entity;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CategoryId{get;set;}
    public string Slug {get;set;} = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } 
    public decimal Price{get;set;}
    public DateTime CreatedAt{get;set;} = DateTime.Now;
    public DateTime? DeletedAt{get;set;}
    
    // navigation properties
    public Category? Category {get;set;}
}