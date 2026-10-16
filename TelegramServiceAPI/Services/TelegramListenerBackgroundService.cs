using TelegramServiceAPI.Core.Services.Handlers;

namespace TelegramServiceAPI.Services;

public class TelegramListenerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelegramListenerBackgroundService> _logger;

    public TelegramListenerBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<TelegramListenerBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фоновый сервис прослушивания Telegram-чата запущен.");

        var offset = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var updateHandler = scope.ServiceProvider.GetRequiredService<TelegramBotUpdateHandler>();

                offset = await updateHandler.HandleAsync(offset, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении обновлений от Telegram. Повтор через 5 секунд...");
                await Task.Delay(5000, stoppingToken);
            }
        }

        _logger.LogInformation("Фоновый сервис прослушивания Telegram-чата остановлен.");
    }
}