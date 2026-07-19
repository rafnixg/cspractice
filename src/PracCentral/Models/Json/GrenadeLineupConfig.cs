namespace PracCentral.Models.Json;

public sealed class GrenadeLineupConfig
{
    public string MapName { get; set; } = string.Empty;
    public List<GrenadeLineupEntry> Lineups { get; set; } = [];
}

public sealed class GrenadeLineupEntry
{
    public string Name { get; set; } = string.Empty;
    public Vector3Dto Origin { get; set; } = new(0, 0, 0);
    public Vector3Dto Velocity { get; set; } = new(0, 0, 0);
}
