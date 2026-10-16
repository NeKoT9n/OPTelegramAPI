using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramServiceAPI.Core.Commands;
using TelegramServiceAPI.Core.Services.Factory;
using TelegramServiceAPI.Model.Abstraction.Handlers;

namespace TelegramServiceAPI.Core.Services.Handlers;

public class PrivateChatHandler(TelegramCommandExecutor commandExecutor) : ITelegramUpdateHandler
{
    public Task<bool> CanHandleAsync(Update update) =>
        Task.FromResult(update.CallbackQuery != null || update.Message?.Chat.Type == ChatType.Private);

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        await commandExecutor.ExecuteAsync(update, cancellationToken);
    }

}