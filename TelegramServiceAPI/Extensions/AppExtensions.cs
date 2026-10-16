using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot;
using TelegramServiceAPI.Core.Commands;
using TelegramServiceAPI.Core.Options;
using TelegramServiceAPI.Core.Services;
using TelegramServiceAPI.Core.Services.Factory;
using TelegramServiceAPI.Core.Services.Handlers;
using TelegramServiceAPI.Core.Services.Handlers.GroupChatHandlers;
using TelegramServiceAPI.Infrastructure;
using TelegramServiceAPI.Model.Abstraction;
using TelegramServiceAPI.Model.Abstraction.Handlers;
using TelegramServiceAPI.Services;

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

        services.AddHostedService<TelegramListenerBackgroundService>();

        services.AddScoped<ITelegramFileSenderService, TelegramFileSenderService>();
        services.AddScoped<ITelegramMessageReSenderService, TelegramMessageReSenderService>();

        AddTelegramBotCommands(services);

        services.AddScoped<TelegramBotUpdateHandler>();
        services.AddScoped<IGroupChatHandler, CommitChatHandler>();

        services.AddTransient<GroupChatHandlerFactory>();

        services.AddTransient<ITelegramUpdateHandler, PrivateChatHandler>();
        services.AddTransient<ITelegramUpdateHandler, GroupChatHandler>();

        return services;
    }

    private static IServiceCollection AddTelegramBotCommands(IServiceCollection services)
    {
        services.AddScoped<TelegramCommandExecutor>();

        services.AddTransient<MainMenuCommand>();
        services.AddTransient<ConnectMenuCommand>();
        services.AddTransient<ConnectPortCommand>();
        services.AddTransient<DisconnectMenuCommand>();
        services.AddTransient<DisconnectPortCommand>();
        services.AddTransient<TestCommand>();

        services.AddSingleton<TelegramCommandFactory>();

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