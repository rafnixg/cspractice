using PracCentral.Config;
using PracCentral.Models.Json;
using PracCentral.Services.Storage;
using PracCentral.Services.Validation;

namespace PracCentral.Modules.Prefire;

public sealed class PrefireRouteManager
{
    private readonly IJsonStorageService _jsonStorageService;
    private readonly IInputSanitizer _inputSanitizer;

    public PrefireRouteManager(IJsonStorageService jsonStorageService, IInputSanitizer inputSanitizer)
    {
        _jsonStorageService = jsonStorageService ?? throw new ArgumentNullException(nameof(jsonStorageService));
        _inputSanitizer = inputSanitizer ?? throw new ArgumentNullException(nameof(inputSanitizer));
    }

    public async Task<PrefireRouteConfig> LoadOrCreateAsync(
        string mapName,
        string routesDirectoryPath,
        CancellationToken cancellationToken = default)
    {
        var path = BuildPath(mapName, routesDirectoryPath);
        if (!File.Exists(path))
        {
            var created = new PrefireRouteConfig
            {
                MapName = mapName,
            };
            await _jsonStorageService.WriteAsync(path, created, cancellationToken);
            return created;
        }

        return await _jsonStorageService.ReadAsync<PrefireRouteConfig>(path, cancellationToken);
    }

    public async Task SaveRouteAsync(
        string mapName,
        string routesDirectoryPath,
        PrefireRouteDefinition route,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(route);
        ValidateRoute(route);

        var config = await LoadOrCreateAsync(mapName, routesDirectoryPath, cancellationToken);
        var existing = config.Routes.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, route.Name, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
        {
            config.Routes.Add(route);
        }
        else
        {
            existing.StartingPoint = route.StartingPoint;
            existing.Nodes = route.Nodes;
        }

        var path = BuildPath(mapName, routesDirectoryPath);
        await _jsonStorageService.WriteAsync(path, config, cancellationToken);
    }

    public async Task<PrefireRouteDefinition> GetRouteAsync(
        string mapName,
        string routesDirectoryPath,
        string routeName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(routeName);
        _inputSanitizer.EnsureSafeIdentifier(routeName);

        var config = await LoadOrCreateAsync(mapName, routesDirectoryPath, cancellationToken);
        return config.Routes.FirstOrDefault(route =>
            string.Equals(route.Name, routeName, StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"Route '{routeName}' was not found.");
    }

    public IReadOnlyList<PrefireSpawnInstruction> BuildSpawnPlan(PrefireRouteDefinition route)
    {
        ArgumentNullException.ThrowIfNull(route);
        ValidateRoute(route);

        return route.Nodes
            .OrderBy(node => node.Wave)
            .ThenBy(node => node.Sequence)
            .Select(node => new PrefireSpawnInstruction(
                node.Wave,
                node.Sequence,
                node.Position,
                node.Angle,
                node.WeaponId,
                node.BotDifficulty))
            .ToArray();
    }

    private string BuildPath(string mapName, string routesDirectoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mapName);
        ArgumentException.ThrowIfNullOrWhiteSpace(routesDirectoryPath);
        var safeMapName = _inputSanitizer.EnsureSafeIdentifier(mapName);
        return Path.Combine(routesDirectoryPath, safeMapName + Constants.PrefireFileExtension);
    }

    private void ValidateRoute(PrefireRouteDefinition route)
    {
        if (string.IsNullOrWhiteSpace(route.Name))
        {
            throw new InvalidOperationException("Route name cannot be empty.");
        }

        _inputSanitizer.EnsureSafeIdentifier(route.Name);

        foreach (var node in route.Nodes)
        {
            if (node.Sequence < 0)
            {
                throw new InvalidOperationException("Route node sequence cannot be negative.");
            }

            if (node.Wave <= 0)
            {
                throw new InvalidOperationException("Route node wave must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(node.WeaponId))
            {
                throw new InvalidOperationException("Route node weapon id cannot be empty.");
            }

            _inputSanitizer.EnsureSafeIdentifier(node.WeaponId);
        }
    }
}
