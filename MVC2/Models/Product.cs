using System.ComponentModel.DataAnnotations;

namespace YourProject.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введіть назву продукту")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Виберіть категорію")]
        public int? CategoryId { get; set; }
    }
}