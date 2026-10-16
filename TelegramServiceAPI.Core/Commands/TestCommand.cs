using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramServiceAPI.Core.Commands.Attributes;
using TelegramServiceAPI.Core.Services.Factory;
using TelegramServiceAPI.Model.Abstraction.Commands;

namespace TelegramServiceAPI.Core.Commands;

[TelegramRoute("/test")]
public class TestCommand(ITelegramBotClient botClient) : IPrivateChatCommand, ICommitChatCommand
{
    private readonly ITelegramBotClient _botClient = botClient;

    public async Task ExecuteAsync(Message? message, CallbackQuery? callbackQuery, CancellationToken cancellationToken)
    {
        var chatId = message?.Chat.Id.ToString() ?? callbackQuery?.Message?.Chat.Id.ToString();
        var messageId = callbackQuery?.Message?.Id;

        if (chatId == null)
            return;

        var text = "*** Тестовое сообщение ***";

        if (callbackQuery != null && messageId.HasValue)
        {
            await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
            await _botClient.EditMessageText(chatId, messageId.Value, text, parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
        }
        else
        {
            await _botClient.SendMessage(chatId, text, parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
        }
    }
}