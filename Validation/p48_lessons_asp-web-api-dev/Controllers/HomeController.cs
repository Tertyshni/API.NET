using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace p48_lessons_asp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        
        /*
        Для того щоб вказати метод запиту для маршруту(ендпоінта) необхідно використовувати аннотації(атрибути)
        Для кожного HTTP методу існує свій атрибут
        CRUD - Create Read Update Delete
        [HttpGet] - для отримання даних (GET) (Read)
        [HttpPost] - для створення нових даних (POST) (Create) (Підтвердження даних(аутентифікація, авторизація))
        [HttpPut] - для оновлення інформації (Повне оновлення, ви оновюєте всі поля в об'єкті)
        [HttpPatch] - для часткового оновлення інформації (Наприклад: оновити тільки ім'я, чи номер телефону, чи назва продукту)
        [HttpDelete] - для видалення даних

        Додатково ви можете вказати маршрут самостійно, для цього використовуєте атрибут [Route]
        [Route("endpointname//*//*")]
        */

        /// <summary>
        /// Стартовий ендпоінт, який повертає привітання
        /// </summary>
        /// <remarks>
        /// Цей метод повертає JSON-об'єкт, з успіхом та вітанням
        /// Приклад запиту: GET /api/home/
        /// </remarks>
        /// <returns>OkObjectResult</returns>
        /// <response code="200">Успіх</response>


        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new
            {
                Status = 200,
                Message = "Welcome to webapi"
            });
        }

        


        [HttpGet]
        [Route("health-check")]
        [SwaggerOperation(
            Summary = "Перевірка стану сервера",
            Description = "Перевіряє стан северу, якщо все гаразд повертає об'єкт(Статус: 200 та повідомлення), інакше ендпоінт не працює"
        // Tags = new []{"Home Controller", "Healt Check"}
        )]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                Status = 200,
                Message = "Server is fine"
            });
        }
        /*
        IGenerator
         generate(int count)
         ProductGenerator: IGenerator
         prodcutGenerator.generate(1000) -> List<Product>
        */
    }
}