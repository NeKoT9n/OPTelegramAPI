using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramServiceAPI.Core.Services.Factory;

public static class TelegramKeyboardFactory
{
    public static InlineKeyboardMarkup CreateMainMenuKeyboard()
        => new(
        [
            [
                InlineKeyboardButton.WithCallbackData("🟢 Подключить", "/connect"),
                InlineKeyboardButton.WithCallbackData("🔴 Отключить", "/disconnect"),
            ]
        ]);

    public static InlineKeyboardMarkup CreateConnectPortsKeyboard() 
        => new(
        [
            [
                InlineKeyboardButton.WithCallbackData("1XXX", "/connect_1"),
                InlineKeyboardButton.WithCallbackData("2XXX", "/connect_2"),
            ],
            [
                InlineKeyboardButton.WithCallbackData("3XXX", "/connect_3"),
                InlineKeyboardButton.WithCallbackData("4XXX", "/connect_4"),
            ],
            [
                InlineKeyboardButton.WithCallbackData("🔙 Назад", "/menu")
            ]
        ]);

    public static InlineKeyboardMarkup CreateBackToMainKeyboard()
        => new(
        [
            [InlineKeyboardButton.WithCallbackData("🔙 В главное меню", "/menu")]
        ]);

    public static InlineKeyboardMarkup CreateDisconnectPortsKeyboard(List<short> activePorts)
    {
        var buttonList = activePorts
            .Select(port => InlineKeyboardButton.WithCallbackData($"{port}XXX", $"/disconnect_{port}"))
            .ToList();

        var rows = buttonList.Chunk(2).Select(chunk => chunk.ToArray()).ToList();

        rows.Add([InlineKeyboardButton.WithCallbackData("🔙 Назад", "/menu")]);

        return new InlineKeyboardMarkup(rows);
    }
}