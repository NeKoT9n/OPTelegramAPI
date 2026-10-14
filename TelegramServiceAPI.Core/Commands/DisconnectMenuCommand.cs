using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramServiceAPI.Infrastructure;
using TelegramServiceAPI.Model.Abstraction;

namespace TelegramServiceAPI.Core.Services.Commands
{
    public class DisconnectMenuCommand(ITelegramBotClient botClient, IServiceProvider serviceProvider) : ITelegramCommand
    {
        public string Key => "menu_disconnect";
        private readonly ITelegramBotClient _botClient = botClient;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task ExecuteAsync(Message? message, CallbackQuery? callbackQuery, CancellationToken cancellationToken)
        {
            var chatId = message?.Chat.Id.ToString() ?? callbackQuery?.Message?.Chat.Id.ToString();
            var messageId = callbackQuery?.Message?.Id;
            if (chatId == null) return;

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TelegramDbContext>();

            var activePorts = await dbContext.Connections
                .Where(e => e.ChatId == chatId && e.IsConnected)
                .Select(e => e.Port)
                .ToListAsync(cancellationToken);

            if (activePorts.Count == 0)
            {
                if (callbackQuery != null)
                {
                    await _botClient.AnswerCallbackQuery(callbackQuery.Id, "❌ У вас нет активных подписок.", cancellationToken: cancellationToken);
                }
                else
                {
                    await _botClient.SendMessage(chatId, "❌ У вас нет активных подписок на порты.", cancellationToken: cancellationToken);
                }
                return;
            }

            var keyboard = TelegramKeyboardFactory.CreateDisconnectPortsKeyboard(activePorts);
            string text = "🔌 **Выберите порт для отключения:**";

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