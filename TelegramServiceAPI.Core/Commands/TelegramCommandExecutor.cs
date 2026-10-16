using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using TelegramServiceAPI.Core.Services.Factory;

namespace TelegramServiceAPI.Core.Commands;

public class TelegramCommandExecutor(
    TelegramCommandFactory commandFactory,
    IServiceScopeFactory scopeFactory)
{
    private readonly TelegramCommandFactory _commandFactory = commandFactory;

    public async Task ExecuteAsync(
        string? commandText,
        Message? message,
        CallbackQuery? callbackQuery,
        CancellationToken cancellationToken)
    {

        using var scope = scopeFactory.CreateScope();

        var command = _commandFactory.GetCommand(commandText, scope.ServiceProvider);

        if (command == null) 
            return;

        await command.ExecuteAsync(message, callbackQuery, cancellationToken);
    }

    public async Task ExecuteAsync(Update update, CancellationToken cancellationToken)
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

        await ExecuteAsync(routeInput, message, callbackQuery, cancellationToken);
    }
}