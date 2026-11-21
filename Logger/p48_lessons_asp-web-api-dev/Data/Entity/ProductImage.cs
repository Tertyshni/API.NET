namespace p48_lessons_asp.Data.Entity;

public class ProductImage
{
    public Guid Id {get;set;} = Guid.NewGuid();
    public Guid ProductId{get;set;}
    public string Url{get;set;} = string.Empty;
    public int Order{get;set;}
    public DateTime CreatedAt{get;set;} = DateTime.UtcNow;
    
    public Product? Product{get;set;}
}