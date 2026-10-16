using Telegram.Bot.Types;

namespace TelegramServiceAPI.Model.Abstraction.Handlers;

public interface IGroupChatHandler
{
    string TargetChatId { get; }
    Task HandleGroupMessageAsync(Update update, CancellationToken cancellationToken);
}
