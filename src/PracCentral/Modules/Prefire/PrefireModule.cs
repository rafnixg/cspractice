using PracCentral.Config;
using PracCentral.Core;
using PracCentral.Core.Contracts;
using PracCentral.Models.Json;

namespace PracCentral.Modules.Prefire;

public sealed class PrefireModule : IPracModule
{
    private PrefireMapConfig? _activeConfig;

    public string Name => "PrefireModule";

    public void Dispose()
    {
        _activeConfig = null;
    }

    public void Load(ModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.Config.Prefire.KickBotsOnLoad)
        {
            context.ServerBridge.ExecuteCommand("bot_kick");
        }

        context.Logger.Info("Prefire module loaded.");
    }

    public void Unload(ModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _activeConfig = null;
        if (context.Config.Prefire.KickBotsOnUnload)
        {
            context.ServerBridge.ExecuteCommand("bot_kick");
        }

        context.Logger.Info("Prefire module unloaded.");
    }

    public async Task LoadMapAsync(ModuleContext context, string mapName, string directoryPath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var safeMapName = context.InputSanitizer.EnsureSafeIdentifier(mapName);
        var filePath = Path.Combine(directoryPath, safeMapName + Constants.PrefireFileExtension);

        var config = await context.JsonStorageService.ReadAsync<PrefireMapConfig>(filePath, cancellationToken);
        _activeConfig = config;
        context.MainThreadDispatcher.Enqueue(() => SpawnNodes(context, config));
    }

    private static void SpawnNodes(ModuleContext context, PrefireMapConfig config)
    {
        foreach (var node in config.Nodes)
        {
            context.ServerBridge.ExecuteCommand("bot_add");
            context.ServerBridge.ExecuteCommand(
                $"echo prefire_node_spawned id={node.Id} pos={node.VectorPos.X},{node.VectorPos.Y},{node.VectorPos.Z} ang={node.VectorAng.Pitch},{node.VectorAng.Yaw},{node.VectorAng.Roll} weapon={node.WeaponId} diff={node.BotDifficulty}");
        }
    }
}
