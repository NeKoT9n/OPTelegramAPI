using Telegram.Bot.Types;

namespace TelegramServiceAPI.Model.Abstraction;

public interface ITelegramMessageReSenderService : ITelegramHandler
{ 
    Task SendTextMessageAsync(string messageText, int? port = null, CancellationToken cancellationToken = default);
    Task HandleIncomingMessageAsync(Message? message, CancellationToken cancellationToken);
}