using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Telegram.Bot;
using TelegramServiceAPI.Core.Services;
using TelegramServiceAPI.Core.Services.Commands;
using TelegramServiceAPI.Infrastructure;
using TelegramServiceAPI.Model.Abstraction;
using TelegramServiceAPI.Options;

namespace TelegramServiceAPI.Extensions;

public static class AppExtensions
{

    public static IServiceCollection AddTelegramCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TelegramSettings>(
            configuration.GetSection(TelegramSettings.SectionName));

        services.Configure<ReSendSettings>(
           configuration.GetSection(ReSendSettings.SectionName));

        services.AddDbContext<TelegramDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("TelegramDbContext")));

        services.AddSingleton<ITelegramBotClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<TelegramSettings>>().Value;
            return new TelegramBotClient(settings.BotToken);
        });

        services.AddScoped<ITelegramFileSenderService, TelegramFileSenderService>();
        services.AddHostedService<TelegramListenerBackgroundService>();
        services.AddScoped<ITelegramPrivateChatService, TelegramPrivateChatService>();
        services.AddScoped<ITelegramMessageReSenderService, TelegramMessageReSenderService>();

        services.AddTransient<ITelegramCommand, MainMenuCommand>();
        services.AddTransient<ITelegramCommand, ConnectMenuCommand>();
        services.AddTransient<ITelegramCommand, DisconnectMenuCommand>();
        services.AddTransient<ITelegramCommand, PortActionCommand>();

        services.AddTransient<TelegramCommandFactory>();
        services.AddTransient<ITelegramPrivateChatService, TelegramPrivateChatService>();

        return services;
    }

    public static IHostBuilder AddTelegramSerilog(this IHostBuilder hostBuilder, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

        hostBuilder.UseSerilog();

        return hostBuilder;
    }

    public static WebApplication UseTelegramCorePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseSwaggerGen();

        return app;
    }
}