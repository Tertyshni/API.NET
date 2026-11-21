using System;

namespace p48_lessons_asp.Models;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    // Для обробки файлів використовується спеціальний інтерфейс
    public IFormFile? productImage{get;set;}

}
