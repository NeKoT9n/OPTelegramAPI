using Telegram.Bot.Types;
using TelegramServiceAPI.Core.Commands;
using TelegramServiceAPI.Model.Abstraction;

namespace TelegramServiceAPI.Core.Services;

public class TelegramPrivateChatService(TelegramCommandExecutor commandExecutor) : ITelegramPrivateChatService
{

    public async Task HandlePrivateMessageAsync(Message message, CancellationToken cancellationToken)
    {
        await commandExecutor.ExecuteAsync(
            message.Text,
            message: message,
            callbackQuery: null,
            cancellationToken: cancellationToken
        );
    }

    public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        await commandExecutor.ExecuteAsync(
            callbackQuery.Data,
            message: null,
            callbackQuery: callbackQuery,
            cancellationToken: cancellationToken
        );
    }
}