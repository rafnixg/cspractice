using PracCentral.Config;
using PracCentral.Models.Json;
using PracCentral.Services.Storage;
using PracCentral.Services.Validation;

namespace PracCentral.Services.Engine;

public sealed class SpawnService
{
    private readonly IJsonStorageService _jsonStorageService;
    private readonly IInputSanitizer _inputSanitizer;

    public SpawnService(IJsonStorageService jsonStorageService, IInputSanitizer inputSanitizer)
    {
        _jsonStorageService = jsonStorageService ?? throw new ArgumentNullException(nameof(jsonStorageService));
        _inputSanitizer = inputSanitizer ?? throw new ArgumentNullException(nameof(inputSanitizer));
    }

    public async Task<SpawnLoadResult> LoadMapSpawnsAsync(
        string mapName,
        string customSpawnsDirectoryPath,
        IReadOnlyList<SpawnPointDto> defaultSpawns,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mapName);
        ArgumentException.ThrowIfNullOrWhiteSpace(customSpawnsDirectoryPath);
        ArgumentNullException.ThrowIfNull(defaultSpawns);

        var safeMapName = _inputSanitizer.EnsureSafeIdentifier(mapName);
        var filePath = Path.Combine(customSpawnsDirectoryPath, safeMapName + Constants.PrefireFileExtension);

        if (!File.Exists(filePath))
        {
            if (defaultSpawns.Count == 0)
            {
                throw new InvalidOperationException("No custom spawns were found and default spawns are empty.");
            }

            return new SpawnLoadResult(SpawnSource.Default, defaultSpawns);
        }

        var loadedConfig = await _jsonStorageService.ReadAsync<SpawnMapConfig>(filePath, cancellationToken);
        var customSpawns = loadedConfig.SpawnPoints ?? [];
        if (customSpawns.Count == 0)
        {
            if (defaultSpawns.Count == 0)
            {
                throw new InvalidOperationException("Custom spawn file was loaded but has no spawn points and no defaults are available.");
            }

            return new SpawnLoadResult(SpawnSource.Default, defaultSpawns);
        }

        return new SpawnLoadResult(SpawnSource.Custom, customSpawns);
    }

    public IReadOnlyList<SpawnPointDto> GetSpawnsForTeam(IReadOnlyList<SpawnPointDto> allSpawns, string team)
    {
        ArgumentNullException.ThrowIfNull(allSpawns);
        ArgumentException.ThrowIfNullOrWhiteSpace(team);

        var normalizedTeam = team.Trim().ToLowerInvariant();
        return allSpawns
            .Where(spawn => string.Equals(spawn.Team, normalizedTeam, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public SpawnPointDto SelectSafestSpawn(
        IReadOnlyList<SpawnPointDto> candidates,
        IReadOnlyList<Vector3Dto> enemyPositions,
        double minimumEnemyDistance)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(enemyPositions);

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException("Cannot select a spawn from an empty candidate list.");
        }

        if (enemyPositions.Count == 0)
        {
            return candidates[0];
        }

        if (minimumEnemyDistance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumEnemyDistance), "Minimum enemy distance cannot be negative.");
        }

        var candidatesWithDistance = candidates
            .Select(candidate => new
            {
                Spawn = candidate,
                MinDistance = enemyPositions.Min(enemy => Distance(candidate.Position, enemy)),
            })
            .ToArray();

        var safeCandidate = candidatesWithDistance
            .Where(item => item.MinDistance >= minimumEnemyDistance)
            .OrderByDescending(item => item.MinDistance)
            .FirstOrDefault();

        if (safeCandidate is not null)
        {
            return safeCandidate.Spawn;
        }

        return candidatesWithDistance
            .OrderByDescending(item => item.MinDistance)
            .First()
            .Spawn;
    }

    private static double Distance(Vector3Dto point, Vector3Dto other)
    {
        var dx = point.X - other.X;
        var dy = point.Y - other.Y;
        var dz = point.Z - other.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}
