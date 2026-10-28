using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var tasks = new List<TaskItem>();
var token = "12345";

app.MapGet("/api/tasks", (HttpRequest request) =>
{
    var completed = request.Query["completed"].ToString();
    var expired = request.Query["expired"].ToString();
    var current = request.Query["current"].ToString();

    if (!ValidBool(completed) || !ValidBool(expired) || !ValidBool(current))
        return Reply(400, "Неправильні параметри", error: "Використовуй true або false");

    var result = tasks.AsEnumerable();

    if (completed != "")
        result = result.Where(t => t.Completed == bool.Parse(completed));

    if (expired == "true")
        result = result.Where(t => !t.Completed && t.DeadlineDate < DateTime.UtcNow);

    if (current == "true")
        result = result.Where(t => !t.Completed && t.DeadlineDate >= DateTime.UtcNow);

    return Reply(200, "Список задач", result.ToList());
});

app.MapGet("/api/task/{id}", (Guid id) =>
{
    var task = tasks.FirstOrDefault(t => t.Id == id);

    if (task == null)
        return Reply(404, "Задачу не знайдено", error: "Неправильний ID");

    return Reply(200, "Задачу знайдено", task);
});

app.MapPost("/api/tasks/create", async (HttpRequest request) =>
{
    if (request.Headers["X-Auth-Token"] != token)
        return Reply(403, "Доступ заборонено", error: "Неправильний токен");

    CreateTask? input;

    try
    {
        input = await JsonSerializer.DeserializeAsync<CreateTask>(
            request.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
    catch (JsonException)
    {
        return Reply(400, "Помилка даних", error: "Неправильний JSON");
    }

    if (input == null || string.IsNullOrWhiteSpace(input.Title))
        return Reply(400, "Помилка даних", error: "Назва задачі обов'язкова");

    if (input.Title.Length > 200)
        return Reply(400, "Помилка даних", error: "Назва задовга");

    if (!DateTime.TryParse(input.DeadlineDate, out var deadline))
        return Reply(400, "Помилка даних", error: "Неправильна дата дедлайну");

    var task = new TaskItem
    {
        Id = Guid.NewGuid(),
        Title = input.Title.Trim(),
        Description = input.Description,
        Completed = false,
        CreatedAt = DateTime.UtcNow,
        DeadlineDate = deadline
    };

    tasks.Add(task);

    return Reply(201, "Задачу створено", task);
});

app.MapDelete("/api/task/{id}", (Guid id, HttpRequest request) =>
{
    if (request.Headers["X-Auth-Token"] != token)
        return Reply(403, "Доступ заборонено", error: "Неправильний токен");

    var task = tasks.FirstOrDefault(t => t.Id == id);

    if (task == null)
        return Reply(404, "Задачу не знайдено", error: "Неправильний ID");

    tasks.Remove(task);

    return Reply(200, "Задачу видалено", task);
});

app.Run();

bool ValidBool(string value)
{
    return value == "" || bool.TryParse(value, out _);
}

IResult Reply(int status, string message, object? data = null, string? error = null)
{
    return Results.Json(new
    {
        status,
        message,
        data,
        error
    }, statusCode: status);
}

class TaskItem
{
    public Guid Id { get; set; }
    public bool Completed { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DeadlineDate { get; set; }
}

class CreateTask
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? DeadlineDate { get; set; }
}
