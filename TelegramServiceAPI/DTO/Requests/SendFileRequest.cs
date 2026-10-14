namespace TelegramServiceAPI.DTO.Requests
{
    public class SendFileRequest
    {
        public IFormFile File { get; set; } = default!;
        public string Description { get; set; } = string.Empty;
    }
}
