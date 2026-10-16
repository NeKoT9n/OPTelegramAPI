using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using TelegramServiceAPI.Core.Services.Factory;
using TelegramServiceAPI.Model.Abstraction.Commands;

namespace TelegramServiceAPI.Core.Commands;

public class TelegramCommandExecutor(
    TelegramCommandFactory commandFactory,
    IServiceScopeFactory scopeFactory)
{
    private readonly TelegramCommandFactory _commandFactory = commandFactory;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    public async Task ExecuteTypedCommandAsync<TCommand>(
        string? commandText,
        Message? message,
        CallbackQuery? callbackQuery,
        CancellationToken cancellationToken) where TCommand : ITelegramCommand
    {
        using var scope = _scopeFactory.CreateScope();

        var command = _commandFactory.GetCommand<TCommand>(commandText, scope.ServiceProvider);

        if (command == null)
            return;

        await command.ExecuteAsync(message, callbackQuery, cancellationToken);
    }

    public async Task ExecuteTypedCommandAsync<TCommand>(Update update, CancellationToken cancellationToken) where TCommand : ITelegramCommand
    {
        string? routeInput = null;
        Message? message = null;
        CallbackQuery? callbackQuery = null;

        if (update.Message != null)
        {
            routeInput = update.Message.Text;
            message = update.Message;
        }
        else if (update.CallbackQuery != null)
        {
            routeInput = update.CallbackQuery.Data;
            callbackQuery = update.CallbackQuery;
            message = update.CallbackQuery.Message;
        }

        await ExecuteTypedCommandAsync<TCommand>(routeInput, message, callbackQuery, cancellationToken);
    }
}