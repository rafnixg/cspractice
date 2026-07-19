using PracCentral.Core;
using PracCentral.Core.Contracts;
using PracCentral.Models.Json;

using PracCentral.Config;

namespace PracCentral.Modules.Grenade;

public sealed class GrenadeModule : IPracModule
{
    private GrenadeThrowSnapshot? _lastThrow;
    private bool _saveLastThrow = true;

    public string Name => "GrenadeModule";

    public void Dispose()
    {
    }

    public void Load(ModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _saveLastThrow = context.Config.Grenade.SaveLastThrow;
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
        if (!_saveLastThrow)
        {
            return;
        }

        _lastThrow = snapshot;
    }

    public GrenadeThrowSnapshot? GetLastThrow()
    {
        return _lastThrow;
    }

    public async Task SaveLastThrowAsync(
        ModuleContext context,
        string mapName,
        string lineupName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (!_saveLastThrow)
        {
            throw new InvalidOperationException("Saving grenade throw snapshots is disabled by configuration.");
        }

        if (_lastThrow is null)
        {
            throw new InvalidOperationException("No throw snapshot was recorded.");
        }

        var safeMapName = context.InputSanitizer.EnsureSafeIdentifier(mapName);
        var safeLineupName = context.InputSanitizer.EnsureSafeIdentifier(lineupName);
        var filePath = Path.Combine(
            context.ModuleDirectoryPath,
            context.Config.Grenade.DataDirectory,
            safeMapName + Constants.PrefireFileExtension);

        GrenadeLineupConfig config;
        if (File.Exists(filePath))
        {
            config = await context.JsonStorageService.ReadAsync<GrenadeLineupConfig>(filePath, cancellationToken);
        }
        else
        {
            config = new GrenadeLineupConfig
            {
                MapName = safeMapName,
            };
        }

        var entry = new GrenadeLineupEntry
        {
            Name = safeLineupName,
            Origin = new Vector3Dto(_lastThrow.OriginX, _lastThrow.OriginY, _lastThrow.OriginZ),
            Velocity = new Vector3Dto(_lastThrow.VelocityX, _lastThrow.VelocityY, _lastThrow.VelocityZ),
        };

        var existing = config.Lineups.FirstOrDefault(lineup =>
            string.Equals(lineup.Name, safeLineupName, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
        {
            config.Lineups.Add(entry);
        }
        else
        {
            existing.Origin = entry.Origin;
            existing.Velocity = entry.Velocity;
        }

        await context.JsonStorageService.WriteAsync(filePath, config, cancellationToken);
    }

    public async Task<IReadOnlyList<GrenadeLineupEntry>> LoadLineupsAsync(
        ModuleContext context,
        string mapName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var safeMapName = context.InputSanitizer.EnsureSafeIdentifier(mapName);
        var filePath = Path.Combine(
            context.ModuleDirectoryPath,
            context.Config.Grenade.DataDirectory,
            safeMapName + Constants.PrefireFileExtension);

        if (!File.Exists(filePath))
        {
            return [];
        }

        var config = await context.JsonStorageService.ReadAsync<GrenadeLineupConfig>(filePath, cancellationToken);
        return config.Lineups;
    }
}

public sealed record GrenadeThrowSnapshot(
    double OriginX,
    double OriginY,
    double OriginZ,
    double VelocityX,
    double VelocityY,
    double VelocityZ);
