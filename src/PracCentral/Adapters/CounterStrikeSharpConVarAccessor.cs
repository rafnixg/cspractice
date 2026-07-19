using PracCentral.Services.Engine;

namespace PracCentral.Adapters;

public sealed class CounterStrikeSharpConVarAccessor : IConVarAccessor
{
    public string GetValue(string conVarName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conVarName);
        try
        {
            var value = Environment.GetEnvironmentVariable($"CVAR_{conVarName}");
            return value ?? string.Empty;
        }
        catch
        {
            throw new KeyNotFoundException($"ConVar not found: {conVarName}");
        }
    }

    public void SetValue(string conVarName, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conVarName);
        ArgumentNullException.ThrowIfNull(value);
        
        try
        {
            Environment.SetEnvironmentVariable($"CVAR_{conVarName}", value);
        }
        catch
        {
            throw new KeyNotFoundException($"ConVar not found: {conVarName}");
        }
    }
}
