using FuelStations.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Development: PostgreSQL у Docker, рядок підключення в appsettings.Development.json.
// Production: Neon, рядок підключення в appsettings.Local.json (не в Git)
// або в змінній оточення ConnectionStrings__Default (вона важливіша за файл).
// Development: PostgreSQL in Docker, connection string in appsettings.Development.json.
// Production: Neon, connection string in git-ignored appsettings.Local.json
// or in the ConnectionStrings__Default environment variable (it wins over the file).
if (!builder.Environment.IsDevelopment())
{
    builder.Configuration
        .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();
}

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Connection string 'Default' is not set. For Production put it into "
        + "appsettings.Local.json or the ConnectionStrings__Default variable.");

// --- Сервіси (контейнер Dependency Injection) ---
// --- Services (Dependency Injection container) ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();

// Документ OpenAPI з коментарями /// <summary>, доступний за /openapi/v1.json.
// OpenAPI document with /// <summary> comments, served at /openapi/v1.json.
builder.Services.AddOpenApi();

var app = builder.Build();

// Пишемо в лог, яка БД використовується (лише хост, ніколи не пароль).
// Log which database is used (host only, never the password).
app.Logger.LogInformation(
    "Environment: {Environment}, database host: {DbHost}",
    app.Environment.EnvironmentName,
    new NpgsqlConnectionStringBuilder(connectionString).Host);

// Застосовуємо міграції, щоб порожня БД (Docker чи Neon) отримала схему.
// Apply pending migrations, so an empty DB (Docker or Neon) gets the schema.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// --- Конвеєр обробки HTTP-запиту (middleware) ---
// --- HTTP pipeline (middleware) ---
// Документація API увімкнена в усіх середовищах: рецензент запускає застосунок
// у Production (Neon) і перевіряє API у Swagger UI за адресою /swagger.
// API docs are enabled in every environment: the reviewer runs the app
// in Production (Neon) and tests the API in Swagger UI at /swagger.
app.MapOpenApi();
app.UseSwaggerUI(options =>
    options.SwaggerEndpoint("/openapi/v1.json", "FuelStations API v1"));

app.MapControllers();

app.Run();
