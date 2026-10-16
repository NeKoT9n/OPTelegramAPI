using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramServiceAPI.Model.Abstraction.Handlers;

namespace TelegramServiceAPI.Core.Services.Handlers;

public class TelegramBotUpdateHandler(
    ITelegramBotClient botClient,
    IEnumerable<ITelegramUpdateHandler> handlers,
    ILogger<TelegramBotUpdateHandler> logger)
{
    public async Task<int> HandleAsync(int currentOffset, CancellationToken cancellationToken)
    {
        var updates = await botClient.GetUpdates(
            offset: currentOffset,
            timeout: 30,
            allowedUpdates: [UpdateType.Message, UpdateType.CallbackQuery],
            cancellationToken: cancellationToken
        );

        var latestOffset = currentOffset;

        foreach (var update in updates)
        {
            latestOffset = update.Id + 1;

            try
            {
                await ProcessSingleUpdateAsync(update, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при обработке апдейта ID: {UpdateId}", update.Id);
            }
        }

        return latestOffset;
    }

    private async Task ProcessSingleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        foreach (var handler in handlers)
        {
            if (await handler.CanHandleAsync(update))
            {
                await handler.HandleAsync(update, cancellationToken);
                return;
            }
        }
    }
}