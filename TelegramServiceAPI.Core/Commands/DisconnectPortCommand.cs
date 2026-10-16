using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramServiceAPI.Core.Commands.Attributes;
using TelegramServiceAPI.Core.Services;
using TelegramServiceAPI.Infrastructure;
using TelegramServiceAPI.Model.Abstraction;

namespace TelegramServiceAPI.Core.Commands;

[TelegramRoute("/disconnect_{port}")]
public class DisconnectPortCommand(
    ITelegramBotClient botClient,
    TelegramDbContext dbContext,
    ILogger<DisconnectPortCommand> logger) : RoutableCommand
{

    public override async Task ExecuteAsync(Message? message, CallbackQuery? callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery?.Message == null) return;

        int port = GetParam<int>("port");

        if (port < 1 || port > 4)
            return;

        var chatId = callbackQuery.Message.Chat.Id.ToString();
        var messageId = callbackQuery.Message.Id;

        var connection = await dbContext.Connections
            .FirstOrDefaultAsync(e => e.ChatId == chatId && e.Port == port, cancellationToken);

        if (connection != null)
        {
            connection.IsConnected = false;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await botClient.AnswerCallbackQuery(callbackQuery.Id, $"❌ Успешно отписан от порта {port}XXX", cancellationToken: cancellationToken);
        await botClient.EditMessageText(
            chatId, messageId,
            $"Вы отписаны от порта **{port}XXX**.",
            parseMode: ParseMode.Markdown,
            replyMarkup: TelegramKeyboardFactory.CreateBackToMainKeyboard(),
            cancellationToken: cancellationToken
        );

        logger.LogInformation("Пользователь {ChatId} отписался от порта {Port}", chatId, port);
    }
}