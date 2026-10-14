using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramServiceAPI.Infrastructure;
using TelegramServiceAPI.Infrastructure.Entities;
using TelegramServiceAPI.Model.Abstraction;

namespace TelegramServiceAPI.Core.Services.Commands
{
    public class PortActionCommand(
        ITelegramBotClient botClient,
        IServiceProvider serviceProvider,
        ILogger<PortActionCommand> logger) : ITelegramCommand
    {
        public string Key => "port_action";
        private readonly ITelegramBotClient _botClient = botClient;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<PortActionCommand> _logger = logger;

        public async Task ExecuteAsync(Message? message, CallbackQuery? callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery?.Data == null || callbackQuery.Message == null) return;

            var chatId = callbackQuery.Message.Chat.Id.ToString();
            var messageId = callbackQuery.Message.Id;
            var data = callbackQuery.Data;

            var parts = data.Split('_');
            if (parts.Length != 2) return;

            string action = parts[0];
            if (!int.TryParse(parts[1], out int port) || port < 1 || port > 4) return;

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TelegramDbContext>();

            var connection = await dbContext.Connections
                .FirstOrDefaultAsync(e => e.ChatId == chatId && e.Port == port, cancellationToken);

            if (action == "conn")
            {
                bool isAlreadyConnected = await dbContext.Connections
                    .AnyAsync(c => c.ChatId == chatId && c.Port == port && c.IsConnected, cancellationToken);

                if (isAlreadyConnected)
                {
                    await _botClient.AnswerCallbackQuery(callbackQuery.Id, $"💡 Вы уже получаете рассылку на порт {port}XXX", cancellationToken: cancellationToken);
                    return;
                }

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

                await _botClient.AnswerCallbackQuery(callbackQuery.Id, $"✅ Успешно подписан на порт {port}XXX", cancellationToken: cancellationToken);
                await _botClient.EditMessageText(
                    chatId, messageId,
                    $"Вы подписаны на порт **{port}XXX**. Теперь коммиты этого порта будут приходить сюда.",
                    parseMode: ParseMode.Markdown,
                    replyMarkup: TelegramKeyboardFactory.CreateBackToMainKeyboard(),
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation("Пользователь {ChatId} подписался на порт {Port}", chatId, port);
            }
            else if (action == "disc")
            {
                if (connection != null)
                {
                    connection.IsConnected = false;
                    await dbContext.SaveChangesAsync(cancellationToken);
                }

                await _botClient.AnswerCallbackQuery(callbackQuery.Id, $"❌ Отписан от порта {port}XXX", cancellationToken: cancellationToken);
                await _botClient.EditMessageText(
                    chatId, messageId,
                    $"Вы отписаны от порта **{port}XXX**.",
                    parseMode: ParseMode.Markdown,
                    replyMarkup: TelegramKeyboardFactory.CreateBackToMainKeyboard(),
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation("Пользователь {ChatId} отписался от порта {Port}", chatId, port);
            }
        }
    }
}