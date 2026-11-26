using System.Globalization;
using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using p48_lessons_asp.Data;
using p48_lessons_asp.Services.Random;
using p48_lessons_asp.Services.Slug;
using Slugify;
using Serilog;
using p48_lessons_asp.Services.StorageService;
using p48_lessons_asp.Services.Hash;
using p48_lessons_asp.Services.Kdf;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using p48_lessons_asp.Services.Token;
using Microsoft.IdentityModel.JsonWebTokens;
using System.IdentityModel.Tokens.Jwt;
using p48_lessons_asp.Options;
using p48_lessons_asp.Data.Enum;
using System.Security.Claims;
using Microsoft.OpenApi;
using DotNetEnv;
using p48_lessons_asp.Services.Email;


namespace p48_lessons_asp
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Env.Load();
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true).Build();

            Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

            try
            {
                Log.Information("Running web-application");
                var builder = WebApplication.CreateBuilder(args);
                ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("en");

                // підключення логера замість стандартного
                builder.Host.UseSerilog();

                builder.Services.AddOptions<JwtOptions>()
                .BindConfiguration(JwtOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

                builder.Services.AddOptions<GmailOptions>()
                .BindConfiguration(GmailOptions.SectionName);

                #region OUR_SERVICES
                builder.Services.AddMediatR(cfg => 
                    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
                builder.Services.AddSingleton<IRandomService, RandomService>();
                builder.Services.AddSingleton<ISlugHelper, SlugHelper>();
                builder.Services.AddSingleton<ISlugService, SlugService>();
                builder.Services.AddSingleton<IStorageService, ServerFileStorageService>();

                builder.Services.AddSingleton<IHashService, Md5HashService>();
                builder.Services.AddSingleton<IKdfService, HashKdfService>();

                builder.Services.AddScoped<IEmailService, GmailService>();

                #endregion
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
                            Name = "dsgnrr",
                            Email = "dsgnrr@rnicrosoft-supprot.com"
                        }
                    });
                    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                    if (File.Exists(xmlPath))
                    {
                        c.IncludeXmlComments(xmlPath);
                    }
                    c.EnableAnnotations();

                    c.AddSecurityDefinition("Bearer" , new OpenApiSecurityScheme
                    {
                        Name="Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter JWT-token(Do not use prefix Bearer)"
                    });

                    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                    });
                });

                // var connectionString = builder.Configuration.GetConnectionString("MSSqlConnection");
                var connectionString = builder.Configuration.GetConnectionString("SQLiteConnection");
                if (connectionString == null)
                {
                    throw new Exception("No connection string. Please provide in appsettings.json");
                }
                try
                {
                    // Прибираю MSSQL для роботи з SQLite, залишу тут, можна буде потім включити
                    // Якщо ви змінюєте провайдера бази даних наприклад MSSQL на MySQL чи будь який інший, міграціїї потрібно створювати знову
                    // builder.Services.AddDbContext<DataContext>(
                    //     options => options.UseSqlServer(connectionString)
                    // );
                    builder.Services.AddDbContext<DataContext>(options =>
                        options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "App permanently stopped. Please check database existing or edit app configuration file");
                    throw new Exception("Getting error while trying to connect to database");
                }


                #region JWT_CONFIGURATION
                var jwtOptions = builder.Configuration
                .GetSection(JwtOptions.SectionName)
                .Get<JwtOptions>() ?? new JwtOptions();

                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // Валідація назви сервіса який створив токен
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,
                        // Валідація отримувача токена
                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,
                        // Валідація життя токену
                        ValidateLifetime = true,

                        // Додавання ключа для підпису + його валідація
                        IssuerSigningKey = 
                            new SymmetricSecurityKey(Encoding
                                .UTF8
                                .GetBytes(jwtOptions.Key)),
                        ValidateIssuerSigningKey = true,
                        // Виключаємо додаткові 5 хвилин після прострочення токену
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var rawToken = (context.SecurityToken as JsonWebToken)?.EncodedToken
                                ?? (context.SecurityToken as JwtSecurityToken)?.RawData;

                            if (string.IsNullOrEmpty(rawToken))
                            {
                                context.Fail("Token is missing");
                                return;
                            }

                            var db = context.HttpContext.RequestServices.GetRequiredService<DataContext>();

                            var journal = await db.TokenJournals
                                .Include(tj=>tj.Token)
                                .Include(tj=>tj.User)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(tj=>
                                    tj.Token!.Token == rawToken &&
                                    tj.Token.Type == TokenType.Access &&
                                    tj.DeactivatedAt == null);
                            if(journal?.User == null || journal.User.DeletedAt != null)
                            {
                                context.Fail("Token rejected or user deleted");
                                return;
                            }

                            var identity = context.Principal?.Identity as ClaimsIdentity;
                            identity?.AddClaim(new Claim(ClaimTypes.Role, journal.User.Role.ToString()));
                        },
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";

                            await context.Response.WriteAsJsonAsync(new
                            {
                                StatusCode = 401,
                                Message=context.AuthenticateFailure?.Message ?? "Required valid Bearer token"
                            });
                        }
                    };
                });

                builder.Services.AddAuthorization();

                builder.Services.AddScoped<ITokenService, JwtTokenService>();
                #endregion


                var app = builder.Build();


                // Мідлвеєр який логує запити
                app.UseSerilogRequestLogging();
                if (app.Environment.IsDevelopment())
                {
                    // створення кінцевої точки, яка буде містити згенерований JSON-файл з описом ендпоінтів
                    app.UseSwagger();
                    // кінцева точка але вже з графічним інтерфейсом
                    app.UseSwaggerUI();
                }

                app.UseStaticFiles();

                app.UseAuthentication();
                app.UseAuthorization();

                // Зв'язує класи контролери з маршрутами
                app.MapControllers();

                app.Run();

            }
            catch (Exception ex) when (ex is not HostAbortedException)
            {
                Console.WriteLine($"REAL ERROR: {ex.InnerException?.Message ?? ex.Message}");
                Log.Fatal(ex, "Application stopped with fatal error");
            }
            finally
            {
                // В будь-якому випадку, закриваємо логер та вивантажуємо буфер на диск
                Log.CloseAndFlush();
            }
        }

    }
}

