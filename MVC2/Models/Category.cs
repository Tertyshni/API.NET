using System.ComponentModel.DataAnnotations;

namespace YourProject.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введіть назву")]
        public string Name { get; set; } = "";
    }
}