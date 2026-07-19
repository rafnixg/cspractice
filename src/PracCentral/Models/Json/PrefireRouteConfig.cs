namespace PracCentral.Models.Json;

public sealed class PrefireRouteConfig
{
    public string MapName { get; set; } = string.Empty;
    public List<PrefireRouteDefinition> Routes { get; set; } = [];
}

public sealed class PrefireRouteDefinition
{
    public string Name { get; set; } = string.Empty;
    public PrefireRouteStartPoint StartingPoint { get; set; } = new();
    public List<PrefireRouteNodeDefinition> Nodes { get; set; } = [];
}

public sealed class PrefireRouteStartPoint
{
    public Vector3Dto Position { get; set; } = new(0, 0, 0);
    public Angle3Dto Angle { get; set; } = new(0, 0, 0);
}

public sealed class PrefireRouteNodeDefinition
{
    public int Sequence { get; set; }
    public int Wave { get; set; } = 1;
    public Vector3Dto Position { get; set; } = new(0, 0, 0);
    public Angle3Dto Angle { get; set; } = new(0, 0, 0);
    public string WeaponId { get; set; } = "weapon_ak47";
    public int BotDifficulty { get; set; } = 1;
}

public sealed record PrefireSpawnInstruction(
    int Wave,
    int Sequence,
    Vector3Dto Position,
    Angle3Dto Angle,
    string WeaponId,
    int BotDifficulty);
