using FastEndpoints;
using FastEndpoints.Swagger;
using Serilog;
using TelegramServiceAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

builder.Host.AddTelegramSerilog(configuration);

try
{
    Log.Information("Запуск Telegram Service API...");

    services.AddTelegramCoreServices(configuration);
    services.AddFastEndpoints();
    services.SwaggerDocument();
    services.AddOpenApi();

    var app = builder.Build();

    app.UseTelegramCorePipeline();
    app.UseFastEndpoints();
    app.UseSwaggerGen();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Приложение неожиданно завершило работу с ошибкой");
}
finally
{
    Log.CloseAndFlush();
}