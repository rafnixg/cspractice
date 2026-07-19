using PracCentral.Core;
using PracCentral.Infrastructure.Logging;
using PracCentral.Models.Json;
using PracCentral.Modules.Grenade;
using PracCentral.Services.Engine;
using PracCentral.Services.Storage;
using PracCentral.Services.Threading;
using PracCentral.Services.Validation;

namespace PracCentral.Tests;

public sealed class GrenadeLineupPersistenceTests : IDisposable
{
    private readonly string _tempDirectory;

    public GrenadeLineupPersistenceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "PracCentralTests", Guid.NewGuid().ToString("N"));
    }

    [Fact]
    public async Task SaveLastThrowAsync_ThenLoadLineupsAsync_PersistsThrow()
    {
        var module = new GrenadeModule();
        var context = BuildContext(saveLastThrow: true);
        module.Load(context);
        module.RecordThrow(new GrenadeThrowSnapshot(1, 2, 3, 4, 5, 6));

        await module.SaveLastThrowAsync(context, "de_mirage", "window_smoke");
        var lineups = await module.LoadLineupsAsync(context, "de_mirage");

        Assert.Single(lineups);
        Assert.Equal("window_smoke", lineups[0].Name);
        Assert.Equal(1, lineups[0].Origin.X);
        Assert.Equal(4, lineups[0].Velocity.X);
    }

    [Fact]
    public async Task SaveLastThrowAsync_WhenDisabled_Throws()
    {
        var module = new GrenadeModule();
        var context = BuildContext(saveLastThrow: false);
        module.Load(context);
        module.RecordThrow(new GrenadeThrowSnapshot(1, 2, 3, 4, 5, 6));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            module.SaveLastThrowAsync(context, "de_mirage", "window_smoke"));
    }

    private ModuleContext BuildContext(bool saveLastThrow)
    {
        return new ModuleContext(
            new MainThreadDispatcher(static callback => callback()),
            new FakeServerBridge(),
            _tempDirectory,
            new JsonStorageService(),
            new InputSanitizer(),
            new EventSubscriptionRegistry(),
            new NullPracLogger(),
            new PluginConfig
            {
                Grenade = new GrenadeModeConfig
                {
                    SaveLastThrow = saveLastThrow,
                    DataDirectory = "data\\grenades",
                },
            });
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    private sealed class FakeServerBridge : IServerBridge
    {
        public void ExecuteCommand(string command)
        {
        }
    }
}
