using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramServiceAPI.Core.Options;
using TelegramServiceAPI.Model.Abstraction;

namespace TelegramServiceAPI.Core.Services;

public class TelegramFileSenderService(ITelegramBotClient botClient, IOptions<TelegramSettings> options) : ITelegramFileSenderService
{
    private readonly ITelegramBotClient _botClient = botClient;
    private readonly TelegramSettings _settings = options.Value;

    public async Task SendFileAsync(Stream fileStream, string fileName, string description, CancellationToken cancellationToken = default)
    {
        var inputFile = new InputFileStream(fileStream, fileName);

        await _botClient.SendDocument(
            chatId: _settings.ChatId,
            document: inputFile,
            caption: description,
            cancellationToken: cancellationToken
        );
    }
}
