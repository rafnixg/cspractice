using PracCentral.Core;
using PracCentral.Core.Contracts;

namespace PracCentral.Modules.Aim;

public sealed class AimModule : IPracModule
{
    private const int HitgroupHead = 1;

    public bool HeadshotOnlyEnabled { get; private set; } = true;
    public bool BulletEconomyEnabled { get; private set; } = true;

    public string Name => "AimModule";

    public void Dispose()
    {
    }

    public void Load(ModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        HeadshotOnlyEnabled = context.Config.Aim.HeadshotOnlyEnabled;
        BulletEconomyEnabled = context.Config.Aim.BulletEconomyEnabled;
        context.Logger.Info("Aim module loaded.");
    }

    public void Unload(ModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Logger.Info("Aim module unloaded.");
    }

    public float ApplyHeadshotOnlyRule(int hitgroup, float incomingDamage)
    {
        if (!HeadshotOnlyEnabled)
        {
            return incomingDamage;
        }

        return hitgroup == HitgroupHead ? incomingDamage : 0.0f;
    }

    public int RefillClipOnKill(int currentClip, int maxClip)
    {
        if (!BulletEconomyEnabled)
        {
            return currentClip;
        }

        return maxClip;
    }
}
