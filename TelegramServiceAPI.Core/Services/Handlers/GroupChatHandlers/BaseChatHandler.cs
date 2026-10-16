using Telegram.Bot.Types;
using TelegramServiceAPI.Core.Commands;
using TelegramServiceAPI.Model.Abstraction.Commands;
using TelegramServiceAPI.Model.Abstraction.Handlers;

namespace TelegramServiceAPI.Core.Services.Handlers.GroupChatHandlers;

public abstract class BaseChatHandler<TCommand>(
    TelegramCommandExecutor commandExecutor) : IGroupChatHandler
    where TCommand : ITelegramCommand
{
    private readonly TelegramCommandExecutor _commandExecutor = commandExecutor;

    public abstract string TargetChatId { get; }

    public virtual async Task HandleGroupMessageAsync(Update update, CancellationToken cancellationToken)
    {
        var message = update.Message ?? update.CallbackQuery?.Message;
        var commandText = update.Message?.Text ?? update.CallbackQuery?.Data;

        if (message == null) 
            return;

        if (!string.IsNullOrWhiteSpace(commandText) && (commandText.StartsWith('/') || update.CallbackQuery != null))
        {
            await HandleCommandAsync(update, cancellationToken);
        }
        else
        {
            await HandleChatMessageAsync(update, cancellationToken);
        }
    }

    protected virtual async Task HandleCommandAsync(Update update, CancellationToken cancellationToken)
    {
        await _commandExecutor.ExecuteTypedCommandAsync<TCommand>(update, cancellationToken);
    }

    public abstract Task HandleChatMessageAsync(Update update, CancellationToken cancellationToken);
}