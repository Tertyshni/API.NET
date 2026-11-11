
using System.ComponentModel.DataAnnotations;

namespace ASP.DTOs;

public class CreateDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public DateTime? DeadlineDate { get; set; }
}
