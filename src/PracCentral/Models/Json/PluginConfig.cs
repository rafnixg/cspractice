namespace PracCentral.Models.Json;

public sealed class PluginConfig
{
    public string Version { get; set; } = "1.0";
    public AimModeConfig Aim { get; set; } = new();
    public PrefireModeConfig Prefire { get; set; } = new();
    public GrenadeModeConfig Grenade { get; set; } = new();
}

public sealed class AimModeConfig
{
    public bool HeadshotOnlyEnabled { get; set; } = true;
    public bool BulletEconomyEnabled { get; set; } = true;
}

public sealed class PrefireModeConfig
{
    public bool KickBotsOnLoad { get; set; } = true;
    public bool KickBotsOnUnload { get; set; } = true;
    public string DataDirectory { get; set; } = "data\\prefire";
    public string RoutesDirectory { get; set; } = "data\\prefire-routes";
    public string SpawnsDirectory { get; set; } = "data\\spawns";
    public double MinimumEnemyDistance { get; set; } = 500;
}

public sealed class GrenadeModeConfig
{
    public bool SaveLastThrow { get; set; } = true;
    public string DataDirectory { get; set; } = "data\\grenades";
}
