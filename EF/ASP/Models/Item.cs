using System.ComponentModel.DataAnnotations;
namespace ASP.Models;

public class Item
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public bool Completed { get; set; } = false;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime DeadlineDate { get; set; }
}
