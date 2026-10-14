using Telegram.Bot.Types;

namespace TelegramServiceAPI.Model.Abstraction
{
    public interface ITelegramCommand
    {
        string Key { get; }
        Task ExecuteAsync(Message? message, CallbackQuery? callbackQuery, CancellationToken cancellationToken);
    }
}