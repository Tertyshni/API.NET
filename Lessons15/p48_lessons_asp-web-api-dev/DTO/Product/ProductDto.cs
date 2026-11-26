using System;

namespace p48_lessons_asp.DTO.Product;

public class ProductDto
{
    public Guid Id { get; set; }
    public Guid CategoryId{get;set;}
    public string CategoryName{get;set;} =  string.Empty;
    public string Slug {get;set;} = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } 
    public decimal Price{get;set;}
}
