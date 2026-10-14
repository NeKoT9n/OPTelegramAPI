using TelegramServiceAPI.Model.Abstraction;

namespace TelegramServiceAPI.Core.Services
{
    public class TelegramCommandFactory(IEnumerable<ITelegramCommand> commands)
    {
        private readonly Dictionary<string, ITelegramCommand> _commandMap = commands.ToDictionary(c => c.Key, c => c);

        public ITelegramCommand GetCommand(string? text, string? callbackData)
        {
            if (!string.IsNullOrEmpty(callbackData))
            {
                if (_commandMap.TryGetValue(callbackData, out var exactMatch))
                    return exactMatch;

                if (callbackData.StartsWith("conn_") || callbackData.StartsWith("disc_"))
                {
                    if (_commandMap.TryGetValue("port_action", out var portActionCommand))
                        return portActionCommand;
                }
            }

            var cmd = text?.Trim().ToLower();
            if (cmd == "/connect" || cmd?.StartsWith("подключить") == true)
            {
                if (_commandMap.TryGetValue("menu_connect", out var connectCmd)) return connectCmd;
            }
            if (cmd == "/disconnect" || cmd?.StartsWith("отключить") == true)
            {
                if (_commandMap.TryGetValue("menu_disconnect", out var disconnectCmd)) return disconnectCmd;
            }

            return _commandMap.TryGetValue("menu_main", out var mainCmd) ? mainCmd : _commandMap.Values.First();
        }
    }
}