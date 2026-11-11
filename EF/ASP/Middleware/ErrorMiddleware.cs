using System.Text.Json;
using ASP.Services;


namespace ASP.Middleware;

public class ErrorMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception)
        {
            context.Response.Clear();
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var response = new Response<object>
            {
                Status = 500,
                Message = "Internal server error",
                Data = null,
                Error = "An unexpected error occurred"
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response)
            );
        }
    }
}
