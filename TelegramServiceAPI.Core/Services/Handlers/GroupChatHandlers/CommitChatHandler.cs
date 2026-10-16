using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using TelegramServiceAPI.Core.Commands;
using TelegramServiceAPI.Core.Options;
using TelegramServiceAPI.Model.Abstraction;
using TelegramServiceAPI.Model.Abstraction.Commands;

namespace TelegramServiceAPI.Core.Services.Handlers.GroupChatHandlers;

public class CommitChatHandler(
    IOptions<TelegramSettings> options,
    ITelegramMessageReSenderService reSenderService,
    ILogger<CommitChatHandler> logger,
    TelegramCommandExecutor commandExecutor) : BaseChatHandler<ICommitChatCommand>(commandExecutor)
{
    private readonly TelegramSettings _settings = options.Value;

    public override string TargetChatId => _settings.ChatId;

    public override async Task HandleChatMessageAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message != null)
        {
            await reSenderService.HandleIncomingMessageAsync(update.Message, cancellationToken);
        }
    }
}