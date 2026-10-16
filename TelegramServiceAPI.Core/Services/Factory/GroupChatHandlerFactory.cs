using TelegramServiceAPI.Model.Abstraction.Handlers;

namespace TelegramServiceAPI.Core.Services.Factory;

public class GroupChatHandlerFactory(IEnumerable<IGroupChatHandler> handlers)
{
    private readonly Dictionary<string, IGroupChatHandler> _handlers = handlers.ToDictionary(c => c.TargetChatId, c => c);

    public bool TryGetHandler(string chatId, out IGroupChatHandler? handler)
        => _handlers.TryGetValue(chatId, out handler);

}
