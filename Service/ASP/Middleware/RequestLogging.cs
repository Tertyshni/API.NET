using System.Collections.Concurrent;

namespace MyWebApi.Middleware
{
    public class RequestLogging
    {
        private readonly RequestDelegate _next;

        public static ConcurrentBag<string> Requests = new();

        public RequestLogging(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string request = $"{DateTime.Now} | {context.Request.Method} | {context.Request.Path}";

            Console.WriteLine(request);

            Requests.Add(request);

            await _next(context);
        }
    }
}