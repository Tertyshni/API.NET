using ASP.Data;
using ASP.DTOs;
using ASP.Models;
using ASP.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;


namespace TasksApi.Controllers;

[ApiController]
[Route("api")]
public class TasksController : ControllerBase
{
    private readonly DbContext _context;
    private readonly IConfiguration _configuration;

    public TasksController(
        DbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    private IActionResult Response<T>(
        int status,
        string message,
        T? data = default,
        string? error = null)
    {
        return StatusCode(status, new Response<T>
        {
            Status = status,
            Message = message,
            Data = data,
            Error = error
        });
    }

    private bool IsAuthorized()
    {
        string? authHeader = Request.Headers.Authorization;

        string? token = authHeader?
            .StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
            ? authHeader.Substring(7).Trim()
            : null;

        string? correctToken = _configuration["Auth:Token"];

        return !string.IsNullOrEmpty(token)
            && !string.IsNullOrEmpty(correctToken)
            && token == correctToken;
    }


    [HttpGet("tasks")]
    public async Task<IActionResult> GetTasks(
        [FromQuery] bool? completed,
        [FromQuery] bool? expired,
        [FromQuery] bool? current)
    {
        IQueryable<Item> query = _context.Tasks;

        if (completed.HasValue)
        {
            query = query.Where(
                t => t.Completed == completed.Value
            );
        }

        DateTime now = DateTime.UtcNow;

        if (expired == true)
        {
            query = query.Where(
                t => !t.Completed && t.DeadlineDate < now
            );
        }

        if (current == true)
        {
            query = query.Where(
                t => !t.Completed && t.DeadlineDate >= now
            );
        }

        var tasks = await query
            .OrderBy(t => t.DeadlineDate)
            .ToListAsync();

        return Response(
            200,
            "Tasks retrieved successfully",
            tasks
        );
    }


    [HttpGet("task/{id}")]
    public async Task<IActionResult> GetTask(string id)
    {
        if (!Guid.TryParse(id, out Guid taskId))
        {
            return Response<object>(
                400,
                "Invalid task ID",
                null,
                "ID must be a valid GUID"
            );
        }

        var task = await _context.Tasks.FindAsync(taskId);

        if (task == null)
        {
            return Response<object>(
                404,
                "Task not found",
                null,
                "Task with this ID does not exist"
            );
        }

        return Response(
            200,
            "Task retrieved successfully",
            task
        );
    }


    [HttpPost("tasks/create")]
    public async Task<IActionResult> CreateTask(
        [FromBody] CreateDto request)
    {
        if (!IsAuthorized())
        {
            return Response<object>(
                403,
                "Access denied",
                null,
                "Invalid or missing authentication token"
            );
        }

        var task = new Item
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description,
            Completed = false,
            CreatedAt = DateTime.UtcNow,
            DeadlineDate = request.DeadlineDate!.Value
        };

        if (string.IsNullOrWhiteSpace(task.Title))
        {
            return Response<object>(
                400,
                "Validation error",
                null,
                "Title cannot be empty"
            );
        }

        _context.Tasks.Add(task);

        await _context.SaveChangesAsync();

        return Response(
            201,
            "Task created successfully",
            task
        );
    }


    [HttpDelete("task/{id}")]
    public async Task<IActionResult> DeleteTask(string id)
    {
        if (!IsAuthorized())
        {
            return Response<object>(
                403,
                "Access denied",
                null,
                "Invalid or missing authentication token"
            );
        }

        if (!Guid.TryParse(id, out Guid taskId))
        {
            return Response<object>(
                400,
                "Invalid task ID",
                null,
                "ID must be a valid GUID"
            );
        }

        var task = await _context.Tasks.FindAsync(taskId);

        if (task == null)
        {
            return Response<object>(
                404,
                "Task not found",
                null,
                "Task with this ID does not exist"
            );
        }

        _context.Tasks.Remove(task);

        await _context.SaveChangesAsync();

        return Response<object>(
            200,
            "Task deleted successfully",
            null
        );
    }
}
