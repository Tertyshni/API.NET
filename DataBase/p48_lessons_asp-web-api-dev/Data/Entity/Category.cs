using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace p48_lessons_asp.Data.Entity;

public class Category
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ParentCategoryId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Category? ParentCategory{get;set;}
    public List<Category>? ChildCategories{get;set;}
    public List<Product>? Products{get;set;}
}