using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramServiceAPI.Core.Services
{
    public static class TelegramKeyboardFactory
    {
        public static InlineKeyboardMarkup CreateMainMenuKeyboard()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🟢 Подключить", "menu_connect"),
                    InlineKeyboardButton.WithCallbackData("🔴 Отключить", "menu_disconnect"),
                }
            });
        }

        public static InlineKeyboardMarkup CreateConnectPortsKeyboard()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("1XXX", "conn_1"),
                    InlineKeyboardButton.WithCallbackData("2XXX", "conn_2"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("3XXX", "conn_3"),
                    InlineKeyboardButton.WithCallbackData("4XXX", "conn_4"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🔙 Назад", "menu_main")
                }
            });
        }

        public static InlineKeyboardMarkup CreateDisconnectPortsKeyboard(List<short> activePorts)
        {
            var buttonList = activePorts
                .Select(port => InlineKeyboardButton.WithCallbackData($"{port}XXX", $"disc_{port}"))
                .ToList();

            var rows = buttonList.Chunk(2).Select(chunk => chunk.ToArray()).ToList();

            // Добавляем кнопку возврата назад в главное меню
            rows.Add(new[] { InlineKeyboardButton.WithCallbackData("🔙 Назад", "menu_main") });

            return new InlineKeyboardMarkup(rows);
        }

        public static InlineKeyboardMarkup CreateBackToMainKeyboard()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("🔙 В главное меню", "menu_main") }
            });
        }
    }
}