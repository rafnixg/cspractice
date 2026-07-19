namespace PracCentral.Models.Json;

public sealed record SpawnMapConfig(string MapName, IReadOnlyList<SpawnPointDto> SpawnPoints);

public sealed record SpawnPointDto(string Team, Vector3Dto Position, Angle3Dto Angle);

public enum SpawnSource
{
    Custom = 0,
    Default = 1,
}

public sealed record SpawnLoadResult(SpawnSource Source, IReadOnlyList<SpawnPointDto> SpawnPoints);
