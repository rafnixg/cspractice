using PracCentral.Config;
using PracCentral.Core;
using PracCentral.Core.Contracts;
using PracCentral.Models.Json;
using PracCentral.Services.Engine;

namespace PracCentral.Modules.Prefire;

public sealed class PrefireModule : IPracModule
{
    private PrefireMapConfig? _activeConfig;
    private PrefireRouteManager? _routeManager;
    private SpawnService? _spawnService;

    public string Name => "PrefireModule";

    public void Dispose()
    {
        _activeConfig = null;
    }

    public void Load(ModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _routeManager ??= new PrefireRouteManager(context.JsonStorageService, context.InputSanitizer);
        _spawnService ??= new SpawnService(context.JsonStorageService, context.InputSanitizer);
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

    public Task LoadMapAsync(ModuleContext context, string mapName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var dataDirectoryPath = Path.Combine(context.ModuleDirectoryPath, context.Config.Prefire.DataDirectory);
        return LoadMapAsync(context, mapName, dataDirectoryPath, cancellationToken);
    }

    public async Task SaveRouteAsync(
        ModuleContext context,
        string mapName,
        PrefireRouteDefinition route,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(route);
        EnsureRouteManager(context);

        await _routeManager!.SaveRouteAsync(
            mapName,
            Path.Combine(context.ModuleDirectoryPath, context.Config.Prefire.RoutesDirectory),
            route,
            cancellationToken);
    }

    public async Task<IReadOnlyList<PrefireSpawnInstruction>> BuildRouteSpawnPlanAsync(
        ModuleContext context,
        string mapName,
        string routeName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        EnsureRouteManager(context);

        var route = await _routeManager!.GetRouteAsync(
            mapName,
            Path.Combine(context.ModuleDirectoryPath, context.Config.Prefire.RoutesDirectory),
            routeName,
            cancellationToken);

        return _routeManager.BuildSpawnPlan(route);
    }

    public async Task<SpawnLoadResult> LoadSpawnsAsync(
        ModuleContext context,
        string mapName,
        IReadOnlyList<SpawnPointDto> defaultSpawns,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(defaultSpawns);
        EnsureSpawnService(context);

        return await _spawnService!.LoadMapSpawnsAsync(
            mapName,
            Path.Combine(context.ModuleDirectoryPath, context.Config.Prefire.SpawnsDirectory),
            defaultSpawns,
            cancellationToken);
    }

    public SpawnPointDto SelectSafestSpawn(
        ModuleContext context,
        IReadOnlyList<SpawnPointDto> candidateSpawns,
        IReadOnlyList<Vector3Dto> enemyPositions)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(candidateSpawns);
        ArgumentNullException.ThrowIfNull(enemyPositions);
        EnsureSpawnService(context);

        return _spawnService!.SelectSafestSpawn(
            candidateSpawns,
            enemyPositions,
            context.Config.Prefire.MinimumEnemyDistance);
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

    private void EnsureRouteManager(ModuleContext context)
    {
        _routeManager ??= new PrefireRouteManager(context.JsonStorageService, context.InputSanitizer);
    }

    private void EnsureSpawnService(ModuleContext context)
    {
        _spawnService ??= new SpawnService(context.JsonStorageService, context.InputSanitizer);
    }
}
