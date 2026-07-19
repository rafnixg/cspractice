using CounterStrikeSharp.API;
using PracCentral.Services.Engine;

namespace PracCentral.Adapters;

public sealed class CounterStrikeSharpServerBridge : IServerBridge
{
    public void ExecuteCommand(string command)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        Server.ExecuteCommand(command);
    }
}
