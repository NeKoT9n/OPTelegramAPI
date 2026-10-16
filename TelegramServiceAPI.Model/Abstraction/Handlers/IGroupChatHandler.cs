using Telegram.Bot.Types;

namespace TelegramServiceAPI.Model.Abstraction.Handlers;

public interface IGroupChatHandler
{
    string TargetChatId { get; }
    Task HandleGroupMessageAsync(Message message, CancellationToken cancellationToken);
}
