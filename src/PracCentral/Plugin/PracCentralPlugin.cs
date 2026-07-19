using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using PracCentral.Adapters;
using PracCentral.Core;
using PracCentral.Infrastructure.Logging;
using PracCentral.Services.Threading;

namespace PracCentral.Plugin;

public sealed class PracCentralPlugin : BasePlugin
{
    private Main? _pracCentral;
    private PracCentralLogger? _logger;

    public override string ModuleName => "PracCentral";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "PracCentral Team";

    public override void Load(bool hotReload)
    {
        _logger = new PracCentralLogger();
        _logger.Info("PracCentral plugin loading...");

        try
        {
            var mainThreadDispatcher = new MainThreadDispatcher(Server.NextFrame);
            var serverBridge = new CounterStrikeSharpServerBridge();
            var conVarAccessor = new CounterStrikeSharpConVarAccessor();

            _pracCentral = new Main(mainThreadDispatcher, serverBridge, conVarAccessor, _logger);
            _logger.Info("PracCentral plugin loaded successfully.");
        }
        catch (Exception exception)
        {
            _logger?.Error($"Failed to load PracCentral: {exception.Message}");
            throw;
        }
    }

    public override void Unload(bool hotReload)
    {
        _logger?.Info("PracCentral plugin unloading...");
        _pracCentral?.StateManager.TransitionTo(PracMode.Idle);
    }
}

public sealed class PracCentralLogger : IPracLogger
{
    public void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[PracCentral ERROR] {message}");
        Console.ResetColor();
    }

    public void Info(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[PracCentral INFO] {message}");
        Console.ResetColor();
    }

    public void Warn(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[PracCentral WARN] {message}");
        Console.ResetColor();
    }
}
