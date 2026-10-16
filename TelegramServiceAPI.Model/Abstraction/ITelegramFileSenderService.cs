namespace TelegramServiceAPI.Model.Abstraction;

public interface ITelegramFileSenderService
{
    Task SendFileAsync(Stream fileStream, string fileName, string description, CancellationToken cancellationToken = default);
}
