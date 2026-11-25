using System;

namespace p48_lessons_asp.DTO.Product;

public class ProductImageDto
{
    public Guid Id {get;set;} = Guid.NewGuid();
    public Guid ProductId{get;set;}
    public string Url{get;set;} = string.Empty;
    public int Order{get;set;}
}
