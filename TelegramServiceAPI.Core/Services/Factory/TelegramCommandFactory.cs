using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.RegularExpressions;
using TelegramServiceAPI.Core.Commands.Attributes;
using TelegramServiceAPI.Model.Abstraction.Commands;

namespace TelegramServiceAPI.Core.Services.Factory;

public class TelegramCommandFactory
{
    private readonly Dictionary<string, Type> _exactMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<(Regex RegexPattern, Type CommandType)> _parameterizedMap = new();
    private readonly Type _defaultCommandType;

    public TelegramCommandFactory()
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


    public ITelegramCommand? GetCommand<TCommand>(string? input, IServiceProvider scopedProvider) where TCommand : ITelegramCommand
    {
        var commandType = ResolveCommandType(input);

        if (commandType == null || !typeof(TCommand).IsAssignableFrom(commandType))
            return null;

        return CreateCommandInstance(commandType, input, scopedProvider);
    }

    public ITelegramCommand GetCommand(string? commandText, IServiceProvider scopedProvider)
    {
        var commandType = ResolveCommandType(commandText) ?? _defaultCommandType;
        return CreateCommandInstance(commandType, commandText, scopedProvider);
    }

    private Type? ResolveCommandType(string? commandText)
    {
        var input = commandText?.Trim();
        if (string.IsNullOrEmpty(input)) return null;

        if (_exactMap.TryGetValue(input, out var exactType))
        {
            return exactType;
        }

        foreach (var (regex, cmdType) in _parameterizedMap)
        {
            if (regex.IsMatch(input))
            {
                return cmdType;
            }
        }

        return null;
    }

    private ITelegramCommand CreateCommandInstance(Type commandType, string? commandText, IServiceProvider scopedProvider)
    {
        var input = commandText?.Trim();
        var parameters = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(input))
        {
            foreach (var (regex, cmdType) in _parameterizedMap)
            {
                if (cmdType == commandType)
                {
                    var match = regex.Match(input);
                    if (match.Success)
                    {
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

        var command = (ITelegramCommand)ActivatorUtilities.CreateInstance(scopedProvider, commandType);

        if (command is IRoutableCommand routableCommand && parameters.Count > 0)
        {
            routableCommand.SetRouteParameters(parameters);
        }

        return command;
    }
}