namespace TelegramServiceAPI.DTO.Requests;

public class SendMessageRequest
{
    public int Port { get; set; } = default!;
    public string Description { get; set; } = string.Empty;
}
