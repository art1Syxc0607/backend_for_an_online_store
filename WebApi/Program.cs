using Application;
using Infrastructure;
using Serilog;
using Serilog.Events;
using WebAPI.Middleware;



//namespace WebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor(); // Как получить HttpContext в сервисе/команде?
builder.Services.Extension(builder.Configuration); // DbContext + Repositories
builder.Services.AddApplication();

builder.Host.UseSerilog((context, config) =>
{
    // 1. Читаем конфигурацию из appsettings.json
    config.ReadFrom.Configuration(context.Configuration);

    // 3. Добавляем File с асинхронностью для ВСЕХ логов
    config.WriteTo.Async(a => a.File(
        path: "logs/all/log-.txt",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Verbose,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    ));

    // 4. Добавляем File с асинхронностью только для Error
    config.WriteTo.Async(a => a.File(
        path: "logs/errors/error-.txt",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Error,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:w3}] {Message:lj}{NewLine}{Exception}"
    ));
});

// реализация In-Memory Cache
builder.Services.AddMemoryCache();


var app = builder.Build();

// Добавляем middleware до всех остальных
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseStaticFiles();

// 2. МАГИЯ: ОДНА СТРОЧКА для логирования времени ВСЕХ запросов
app.UseSerilogRequestLogging(); // Важно: поставить ДО эндпоинтов!

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthorization();

app.MapControllers();

app.Run();
