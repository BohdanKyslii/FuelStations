using FuelStations.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Development: PostgreSQL у Docker, рядок підключення в appsettings.Development.json.
// Production: Neon, рядок підключення в appsettings.Local.json (не в Git)
// або в змінній оточення ConnectionStrings__Default (вона важливіша за файл).
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
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();

// Документ OpenAPI з коментарями /// <summary>, доступний за /openapi/v1.json.
builder.Services.AddOpenApi();

var app = builder.Build();

// Пишемо в лог, яка БД використовується (лише хост, ніколи не пароль).
app.Logger.LogInformation(
    "Environment: {Environment}, database host: {DbHost}",
    app.Environment.EnvironmentName,
    new NpgsqlConnectionStringBuilder(connectionString).Host);

// Застосовуємо міграції, щоб порожня БД (Docker чи Neon) отримала схему.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// --- Конвеєр обробки HTTP-запиту (middleware) ---
// Документація API увімкнена в усіх середовищах: рецензент запускає застосунок
// у Production (Neon) і перевіряє API у Swagger UI за адресою /swagger.
app.MapOpenApi();
app.UseSwaggerUI(options =>
    options.SwaggerEndpoint("/openapi/v1.json", "FuelStations API v1"));

app.MapControllers();

app.Run();
