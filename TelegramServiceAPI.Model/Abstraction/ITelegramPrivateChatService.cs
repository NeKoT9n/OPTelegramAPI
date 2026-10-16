using Telegram.Bot.Types;

namespace TelegramServiceAPI.Model.Abstraction;

public interface ITelegramPrivateChatService : ITelegramHandler
{
    Task HandlePrivateMessageAsync(Message message, CancellationToken cancellationToken);
    Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken);
}