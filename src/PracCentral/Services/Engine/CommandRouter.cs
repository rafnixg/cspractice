using PracCentral.Core;

namespace PracCentral.Services.Engine;

public sealed class CommandRouter
{
    private readonly IReadOnlyDictionary<string, PracMode> _commandToMode;
    private readonly IReadOnlyDictionary<string, string> _commandAliases;

    public CommandRouter(
        IReadOnlyDictionary<string, PracMode> commandToMode,
        IReadOnlyDictionary<string, string> commandAliases)
    {
        _commandToMode = commandToMode ?? throw new ArgumentNullException(nameof(commandToMode));
        _commandAliases = commandAliases ?? throw new ArgumentNullException(nameof(commandAliases));
    }

    public bool TryResolveMode(string commandText, out PracMode mode)
    {
        mode = PracMode.Idle;
        if (string.IsNullOrWhiteSpace(commandText))
        {
            return false;
        }

        if (_commandToMode.TryGetValue(commandText, out mode))
        {
            return true;
        }

        if (!_commandAliases.TryGetValue(commandText, out var mappedCommand))
        {
            return false;
        }

        return _commandToMode.TryGetValue(mappedCommand, out mode);
    }
}
