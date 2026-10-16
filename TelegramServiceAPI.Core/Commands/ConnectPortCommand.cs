using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramServiceAPI.Core.Commands.Attributes;
using TelegramServiceAPI.Core.Services;
using TelegramServiceAPI.Core.Services.Factory;
using TelegramServiceAPI.Infrastructure;
using TelegramServiceAPI.Infrastructure.Entities;
using TelegramServiceAPI.Model.Abstraction;

namespace TelegramServiceAPI.Core.Commands;

[TelegramRoute("/connect_{port}")]
public class ConnectPortCommand(
    ITelegramBotClient botClient,
    TelegramDbContext dbContext,
    ILogger<ConnectPortCommand> logger) : RoutableCommand
{
    public override async Task ExecuteAsync(Message? message, CallbackQuery? callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery?.Message == null) 
            return;

        int port = GetParam<int>("port");

        if (port < 1 || port > 4) 
            return;

        var chatId = callbackQuery.Message.Chat.Id.ToString();
        var messageId = callbackQuery.Message.Id;

        bool isAlreadyConnected = await dbContext.Connections
            .AnyAsync(c => c.ChatId == chatId && c.Port == port && c.IsConnected, cancellationToken);

        if (isAlreadyConnected)
        {
            await botClient.AnswerCallbackQuery(callbackQuery.Id, $"💡 Вы уже подписаны на порт {port}XXX", cancellationToken: cancellationToken);
            return;
        }

        var connection = await dbContext.Connections
            .FirstOrDefaultAsync(e => e.ChatId == chatId && e.Port == port, cancellationToken);

        if (connection == null)
        {
            connection = new ConnectionEntity { ChatId = chatId, Port = (short)port, IsConnected = true };
            dbContext.Connections.Add(connection);
        }
        else
        {
            connection.IsConnected = true;
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        await botClient.AnswerCallbackQuery(callbackQuery.Id, $"✅ Успешно подписан на порт {port}XXX", cancellationToken: cancellationToken);
        await botClient.EditMessageText(
            chatId, messageId,
            $"Вы подписаны на порт **{port}XXX**.",
            parseMode: ParseMode.Markdown,
            replyMarkup: TelegramKeyboardFactory.CreateBackToMainKeyboard(),
            cancellationToken: cancellationToken
        );

        logger.LogInformation("Пользователь {ChatId} подписался на порт {Port}", chatId, port);
    }
}