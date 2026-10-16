using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramServiceAPI.Core.Commands.Attributes;
using TelegramServiceAPI.Core.Services.Factory;
using TelegramServiceAPI.Infrastructure;
using TelegramServiceAPI.Model.Abstraction.Commands;

namespace TelegramServiceAPI.Core.Commands;

[TelegramRoute("/disconnect")]
public class DisconnectMenuCommand(
    ITelegramBotClient botClient,
    TelegramDbContext dbContext) : IPrivateChatCommand
{

    public async Task ExecuteAsync(Message? message, CallbackQuery? callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery?.Message == null) 
            return;

        var chatId = callbackQuery.Message.Chat.Id.ToString();

        var activePorts = await dbContext.Connections
            .AsNoTracking()
            .Where(c => c.ChatId == chatId && c.IsConnected)
            .Select(c => (short)c.Port)
            .ToListAsync(cancellationToken);

        var keyboard = TelegramKeyboardFactory.CreateDisconnectPortsKeyboard(activePorts);
        var text = activePorts.Count > 0
            ? "Выберите порт для отключения:"
            : "У вас нет активных подписок на порты.";

        await botClient.EditMessageText(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.Id,
            text: text,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken
        );
    }
}