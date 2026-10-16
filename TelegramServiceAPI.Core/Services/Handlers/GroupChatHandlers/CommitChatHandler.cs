using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using TelegramServiceAPI.Model.Abstraction;
using TelegramServiceAPI.Model.Abstraction.Handlers;
using TelegramServiceAPI.Options;

namespace TelegramServiceAPI.Core.Services.Handlers.GroupChatHandlers;

public class CommitChatHandler(
    IOptions<TelegramSettings> options,
    ITelegramMessageReSenderService reSenderService,
    ILogger<CommitChatHandler> logger) : IGroupChatHandler
{
    private readonly TelegramSettings _settings = options.Value;

    public string TargetChatId => _settings.ChatId;

    public async Task HandleGroupMessageAsync(Message message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message.Text)) return;

        if (message.Text.StartsWith('/'))
        {
            logger.LogInformation("Получена команда в чате коммитов: {Text}", message.Text);
            return;
        }

        await reSenderService.HandleIncomingMessageAsync(message, cancellationToken);
    }
}