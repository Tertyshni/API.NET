using System;

namespace p48_lessons_asp.Models;

public class GetProductQueryModel
{
    public Guid? Id { get; set; }
    public string? searchQuery { get; set; }
    public int? rating { get; set; }
    public string? orderBy { get; set; }
    public int pageSize { get; set; }
    public int pageIndex { get; set; }

    public override string ToString()
    {
        string result = "\n";
        result += Id.HasValue ? $"Id: {Id.Value}\n" : "";
        result += searchQuery is not null ? $"Query: {searchQuery}\n" : "";
        result += rating.HasValue ? $"Rating: {rating.Value}\n" : "";
        result += orderBy is not null ? $"Order by: {orderBy}\n" : "";
        result += $"Page size: {pageSize}\nPage index: {pageIndex}\n";
        return result;
    }
}
