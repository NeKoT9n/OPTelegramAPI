namespace TelegramServiceAPI.Options
{
    public class TelegramSettings
    {
        public const string SectionName = "TelegramSettings";

        public string BotToken { get; set; } = string.Empty;
        public string ChatId { get; set; } = string.Empty;
    }
}
