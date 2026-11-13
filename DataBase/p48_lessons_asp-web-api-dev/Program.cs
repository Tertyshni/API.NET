using System.Globalization;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using p48_lessons_asp.Data;
using p48_lessons_asp.Models;
using p48_lessons_asp.Services.Random;
using p48_lessons_asp.Services.Slug;
using Slugify;

namespace p48_lessons_asp
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("en");
            builder.Services.AddSingleton<IRandomService, RandomService>();
            builder.Services.AddSingleton<ISlugHelper, SlugHelper>();
            builder.Services.AddSingleton<ISlugService, SlugService>();

            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            builder.Services.AddValidatorsFromAssemblyContaining<Program>();
            // builder.Services.AddSingleton<IGenerator<Product>, ProductGenerator>();
            // Додаємо сервіс, який буде перенаправляти запити на контролери
            builder.Services.AddControllers();
            // сервіс який збирає метадані про всі ендпоінти(маршрути) у додатку
            builder.Services.AddEndpointsApiExplorer();
            // сервіс генератор, який генрує специфікацію(JSON-файл), на основі якого будується веб-сторінка, з усіма ендпоінтами
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
                {
                    Title = "ASP.Net Core WebAPI Lessons",
                    Version = "v1.0",
                    Description = "Documentation for REST API on ASP.Net Core",
                    Contact = new Microsoft.OpenApi.OpenApiContact
                    {
                        Name="dsgnrr",
                        Email="dsgnrr@rnicrosoft-supprot.com"
                    }
                });
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

                c.IncludeXmlComments(xmlPath);
                c.EnableAnnotations();
            });

            var connectionString = builder.Configuration.GetConnectionString("MSSqlConnection");
            if(connectionString == null)
            {
                throw new Exception("No connection string. Please provide in appsettings.json");
            }
            try
            {
                builder.Services.AddDbContext<DataContext>(
                    options=>options.UseSqlServer(connectionString)
                );
            }
            catch(Exception)
            {
                throw new Exception("Getting error while trying to connect to database");
            }
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                // створення кінцевої точки, яка буде містити згенерований JSON-файл з описом ендпоінтів
                app.UseSwagger();
                // кінцева точка але вже з графічним інтерфейсом
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            // Зв'язує класи контролери з маршрутами
            app.MapControllers();

            app.Run();
            
        }
        
    }
}

