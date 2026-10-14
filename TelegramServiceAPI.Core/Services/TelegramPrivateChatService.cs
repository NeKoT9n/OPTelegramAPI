using Telegram.Bot.Types;
using TelegramServiceAPI.Model.Abstraction;

namespace TelegramServiceAPI.Core.Services
{
    public class TelegramPrivateChatService(TelegramCommandFactory commandFactory) : ITelegramPrivateChatService
    {
        private readonly TelegramCommandFactory _commandFactory = commandFactory;

        public async Task HandlePrivateMessageAsync(Message message, CancellationToken cancellationToken)
        {
            var command = _commandFactory.GetCommand(message.Text, null);
            await command.ExecuteAsync(message, null, cancellationToken);
        }

        public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var command = _commandFactory.GetCommand(null, callbackQuery.Data);
            await command.ExecuteAsync(null, callbackQuery, cancellationToken);
        }
    }
}