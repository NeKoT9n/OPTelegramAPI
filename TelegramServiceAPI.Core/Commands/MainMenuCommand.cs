using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramServiceAPI.Core.Commands.Attributes;
using TelegramServiceAPI.Core.Services.Factory;
using TelegramServiceAPI.Model.Abstraction.Commands;

namespace TelegramServiceAPI.Core.Commands;

[TelegramRoute("/start", "/menu")]
public class MainMenuCommand(ITelegramBotClient botClient) : IPrivateChatCommand
{
    public async Task ExecuteAsync(Message? message, CallbackQuery? callbackQuery, CancellationToken cancellationToken)
    {
        var chatId = message?.Chat.Id ?? callbackQuery?.Message?.Chat.Id;
        if (chatId == null)
            return;

        var keyboard = TelegramKeyboardFactory.CreateMainMenuKeyboard();
        var text = "🎛 **Панель управления подписками на коммиты**\n\nВыберите нужное действие:";

        if (callbackQuery?.Message != null)
        {
            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
            await botClient.EditMessageText(chatId, callbackQuery.Message.Id, text, parseMode: ParseMode.Markdown, replyMarkup: keyboard, cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendMessage(chatId, text, parseMode: ParseMode.Markdown, replyMarkup: keyboard, cancellationToken: cancellationToken);
        }
    }
}