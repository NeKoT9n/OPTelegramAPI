using Telegram.Bot.Types;

namespace TelegramServiceAPI.Model.Abstraction;

public interface ITelegramCommand
{
    Task ExecuteAsync(Message? message, CallbackQuery? callbackQuery, CancellationToken cancellationToken);
}

public interface IRoutableCommand : ITelegramCommand
{
    void SetRouteParameters(IReadOnlyDictionary<string, string> parameters);
}

public abstract class RoutableCommand : IRoutableCommand
{
    private IReadOnlyDictionary<string, string> _parameters = new Dictionary<string, string>();

    public void SetRouteParameters(IReadOnlyDictionary<string, string> parameters)
    {
        _parameters = parameters;
    }

    protected T? GetParam<T>(string name) where T : IParsable<T>
    {
        if (_parameters.TryGetValue(name, out var val) && T.TryParse(val, null, out var result))
        {
            return result;
        }
        return default;
    }

    public abstract Task ExecuteAsync(Message? message, CallbackQuery? callbackQuery, CancellationToken cancellationToken);
}