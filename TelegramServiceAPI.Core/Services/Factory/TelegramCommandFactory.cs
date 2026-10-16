using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.RegularExpressions;
using TelegramServiceAPI.Core.Commands.Attributes;
using TelegramServiceAPI.Model.Abstraction;

namespace TelegramServiceAPI.Core.Services.Factory;

public class TelegramCommandFactory
{
    private readonly Dictionary<string, Type> _exactMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<(Regex RegexPattern, Type CommandType)> _parameterizedMap = new();
    private readonly Type _defaultCommandType;

    public TelegramCommandFactory(IServiceProvider serviceProvider)
    {
        Type[] types;
        try
        {
            types = Assembly.GetExecutingAssembly().GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(t => t != null).ToArray()!;
        }

        var commandTypes = types
            .Where(t => typeof(ITelegramCommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();

        if (commandTypes.Count == 0)
        {
            throw new InvalidOperationException("Не найдено ни одной реализации ITelegramCommand в текущей сборке.");
        }

        foreach (var cmdType in commandTypes)
        {
            foreach (var attr in cmdType.GetCustomAttributes<TelegramRouteAttribute>())
            {
                foreach (var pattern in attr.Patterns)
                {
                    if (pattern.Contains('{') && pattern.Contains('}'))
                    {
                        var regexPattern = Regex.Replace(pattern, @"\{([a-zA-Z0-9_]+)\}", "(?<$1>[^/]+)");
                        var regex = new Regex($"^{regexPattern}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                        _parameterizedMap.Add((regex, cmdType));
                    }
                    else
                    {
                        _exactMap[pattern] = cmdType;
                    }
                }
            }
        }

        _defaultCommandType = _exactMap.TryGetValue("/start", out var startType)
                              ? startType
                              : commandTypes[0];
    }

    public ITelegramCommand GetCommand(string? commandText, IServiceProvider scopedProvider)
    {
        var input = commandText?.Trim();
        var targetType = _defaultCommandType;
        var parameters = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(input))
        {
            if (_exactMap.TryGetValue(input, out var exactType))
            {
                targetType = exactType;
            }
            else
            {
                foreach (var (regex, cmdType) in _parameterizedMap)
                {
                    var match = regex.Match(input);
                    if (match.Success)
                    {
                        targetType = cmdType;

                        foreach (string groupName in regex.GetGroupNames())
                        {
                            if (groupName != "0")
                            {
                                parameters[groupName] = match.Groups[groupName].Value;
                            }
                        }
                        break;
                    }
                }
            }
        }

        var command = (ITelegramCommand)ActivatorUtilities.CreateInstance(scopedProvider, targetType);

        if (command is IRoutableCommand routableCommand && parameters.Count > 0)
            routableCommand.SetRouteParameters(parameters);

        return command;
    }
}