using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramServiceAPI.Core.Services.Factory;
using TelegramServiceAPI.Model.Abstraction.Handlers;

namespace TelegramServiceAPI.Core.Services.Handlers;

public class GroupChatHandler(
    GroupChatHandlerFactory factory,
    ILogger<GroupChatHandler> logger) : ITelegramUpdateHandler
{
    public Task<bool> CanHandleAsync(Update update) =>
        Task.FromResult(update.Message?.Chat.Type is ChatType.Group or ChatType.Supergroup);

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        var chatId = update.Message!.Chat.Id;

        if (factory.TryGetHandler(chatId.ToString(), out var handler) && handler != null)
        {
            await handler.HandleGroupMessageAsync(update.Message, cancellationToken);
        }
        else
        {
            logger.LogDebug("Сообщение из группового чата {ChatId} проигнорировано: нет зарегистрированного обработчика.", chatId);
        }
    }

}