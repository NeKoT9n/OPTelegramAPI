using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramServiceAPI.Infrastructure;
using TelegramServiceAPI.Model.Abstraction;
using TelegramServiceAPI.Options;

namespace TelegramServiceAPI.Core.Services;

public class TelegramMessageReSenderService(
    ITelegramBotClient botClient,
    IOptions<TelegramSettings> options,
    TelegramDbContext dbContext,
    ILogger<TelegramMessageReSenderService> logger,
    IServiceProvider serviceProvider) : ITelegramMessageReSenderService
{
    private readonly ITelegramBotClient _botClient = botClient;
    private readonly TelegramSettings _settings = options.Value;
    private readonly TelegramDbContext _dbContext = dbContext;
    private readonly ILogger<TelegramMessageReSenderService> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task SendTextMessageAsync(string messageText, int? port = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(messageText)) return;

        int targetPort = port ?? ExtractPortFromText(messageText);

        if (targetPort > 0)
        {
            var subscribers = await _dbContext.Connections
                .Where(e => e.Port == targetPort && e.IsConnected)
                .ToListAsync(cancellationToken);

            if (subscribers.Count == 0)
                return;

            foreach (var subscriber in subscribers)
            {
                await _botClient.SendMessage(
                    chatId: subscriber.ChatId,
                    text: messageText,
                    cancellationToken: cancellationToken
                );
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(_settings.ChatId))
            {
                await _botClient.SendMessage(
                    chatId: _settings.ChatId,
                    text: messageText,
                    cancellationToken: cancellationToken
                );
            }
        }
    }

    public async Task HandleIncomingMessageAsync(Message? message, CancellationToken cancellationToken)
    {
        if (message?.Text == null)
            return;

        if (message.Chat.Id.ToString() != _settings.ChatId)
            return;

        _logger.LogInformation("Получено сообщение в чате коммитов: {Text}", message.Text);

        int targetPort = ExtractPortFromText(message.Text);

        if (targetPort <= 0)
        {
            _logger.LogWarning("Не удалось определить порт в тексте сообщения.");
            return;
        }

        var subscribers = await _dbContext.Connections
            .Where(e => e.Port == targetPort && e.IsConnected)
            .ToListAsync(cancellationToken);

        if (subscribers.Count == 0)
        {
            _logger.LogInformation("Для порта {Port} не найдено активных подписчиков.", targetPort);
            return;
        }

        foreach (var subscriber in subscribers)
        {
            try
            {
                await _botClient.ForwardMessage(
                    chatId: subscriber.ChatId,
                    fromChatId: message.Chat.Id,
                    messageId: message.Id,
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation("Сообщение (ID: {MessageId}) успешно переслано в личный чат {ChatId}", message.Id, subscriber.ChatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось переслать сообщение в личный чат {ChatId}", subscriber.ChatId);
            }
        }
    }

    private int ExtractPortFromText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;

        var match = System.Text.RegularExpressions.Regex.Match(text, @"\b(\d)(\d{3})\b");

        if (match.Success)
        {
            int firstDigit = int.Parse(match.Groups[1].Value);

            if (firstDigit >= 1 && firstDigit <= 4)
            {
                return firstDigit;
            }
        }

        foreach (var c in text)
        {
            if (char.IsDigit(c))
            {
                int digit = int.Parse(c.ToString());
                if (digit >= 1 && digit <= 4)
                    return digit;
            }
        }

        return 0;
    }
}