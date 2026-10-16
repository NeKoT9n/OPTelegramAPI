namespace TelegramServiceAPI.Infrastructure.Entities;

public class ConnectionEntity
{
    public Guid Id { get ; set; }
    public bool IsConnected { get; set; }
    public short Port { get; set; }
    public string ChatId { get; set; }
}
