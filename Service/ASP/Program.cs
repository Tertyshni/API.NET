using MyWebApi.Services;
using MyWebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ShoppingService>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<RequestLogging>();

app.MapGet("/", () => "API працює!");

app.MapGet("/requests", () =>
{
    return RequestLogging.Requests;
});

app.MapGet("/add/{product}", (string product, ShoppingService cart) =>
{
    cart.AddProduct(product);

    return $"Товар {product} додано!";
});

app.MapGet("/cart", (ShoppingService cart) =>
{
    return cart.GetProducts();
});

app.Run();