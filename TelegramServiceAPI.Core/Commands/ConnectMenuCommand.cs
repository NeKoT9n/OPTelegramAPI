using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramServiceAPI.Model.Abstraction;

namespace TelegramServiceAPI.Core.Services.Commands
{
    public class ConnectMenuCommand(ITelegramBotClient botClient) : ITelegramCommand
    {
        public string Key => "menu_connect";
        private readonly ITelegramBotClient _botClient = botClient;

        public async Task ExecuteAsync(Message? message, CallbackQuery? callbackQuery, CancellationToken cancellationToken)
        {
            var chatId = message?.Chat.Id.ToString() ?? callbackQuery?.Message?.Chat.Id.ToString();
            var messageId = callbackQuery?.Message?.Id;
            if (chatId == null) return;

            var keyboard = TelegramKeyboardFactory.CreateConnectPortsKeyboard();
            string text = "🎛 **Выберите порт для подключения:**";

            if (callbackQuery != null && messageId.HasValue)
            {
                await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                await _botClient.EditMessageText(chatId, messageId.Value, text, parseMode: ParseMode.Markdown, replyMarkup: keyboard, cancellationToken: cancellationToken);
            }
            else
            {
                await _botClient.SendMessage(chatId, text, parseMode: ParseMode.Markdown, replyMarkup: keyboard, cancellationToken: cancellationToken);
            }
        }
    }
}