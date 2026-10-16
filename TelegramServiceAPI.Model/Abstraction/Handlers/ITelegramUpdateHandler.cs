using Telegram.Bot.Types;
namespace TelegramServiceAPI.Model.Abstraction.Handlers;

public interface ITelegramUpdateHandler
{
    Task<bool> CanHandleAsync(Update update);
    Task HandleAsync(Update update, CancellationToken cancellationToken);
}
