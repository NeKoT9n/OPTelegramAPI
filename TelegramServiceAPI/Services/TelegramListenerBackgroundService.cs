using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramServiceAPI.Model.Abstraction;

namespace TelegramServiceAPI.Core.Services
{
    public class TelegramListenerBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TelegramListenerBackgroundService> _logger;

        public TelegramListenerBackgroundService(
            IServiceScopeFactory scopeFactory,
            ITelegramBotClient botClient,
            ILogger<TelegramListenerBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _botClient = botClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Фоновый сервис прослушивания Telegram-чата запущен (параллельный режим).");

            int offset = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var updates = await _botClient.GetUpdates(
                        offset: offset,
                        timeout: 30,
                        allowedUpdates: [UpdateType.Message, UpdateType.CallbackQuery],
                        cancellationToken: stoppingToken
                    );

                    foreach (var update in updates)
                    {
                        offset = update.Id + 1;

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                using var scope = _scopeFactory.CreateScope();

                                if (update.Message != null)
                                {
                                    if (update.Message.Chat.Type == ChatType.Private)
                                    {
                                        var privateHandler = scope.ServiceProvider.GetRequiredService<ITelegramPrivateChatService>();
                                        await privateHandler.HandlePrivateMessageAsync(update.Message, stoppingToken);
                                    }
                                    else
                                    {
                                        var reSenderService = scope.ServiceProvider.GetRequiredService<ITelegramMessageReSenderService>();
                                        await reSenderService.HandleIncomingMessageAsync(update.Message, stoppingToken);
                                    }
                                }
                                else if (update.CallbackQuery != null)
                                {
                                    var privateHandler = scope.ServiceProvider.GetRequiredService<ITelegramPrivateChatService>();
                                    await privateHandler.HandleCallbackQueryAsync(update.CallbackQuery, stoppingToken);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Ошибка при параллельной обработке апдейта ID: {UpdateId}", update.Id);
                            }
                        }, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при получении обновлений от Telegram.");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }
    }
}