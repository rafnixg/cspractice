using PracCentral.Core;
using PracCentral.Core.Contracts;

namespace PracCentral.Modules.Grenade;

public sealed class GrenadeModule : IPracModule
{
    private GrenadeThrowSnapshot? _lastThrow;

    public string Name => "GrenadeModule";

    public void Dispose()
    {
    }

    public void Load(ModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Logger.Info("Grenade module loaded.");
    }

    public void Unload(ModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _lastThrow = null;
        context.Logger.Info("Grenade module unloaded.");
    }

    public void RecordThrow(GrenadeThrowSnapshot snapshot)
    {
        _lastThrow = snapshot;
    }

    public GrenadeThrowSnapshot? GetLastThrow()
    {
        return _lastThrow;
    }
}

public sealed record GrenadeThrowSnapshot(
    double OriginX,
    double OriginY,
    double OriginZ,
    double VelocityX,
    double VelocityY,
    double VelocityZ);
