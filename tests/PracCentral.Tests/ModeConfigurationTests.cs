using PracCentral.Core;
using PracCentral.Infrastructure.Logging;
using PracCentral.Models.Json;
using PracCentral.Modules.Aim;
using PracCentral.Modules.Grenade;
using PracCentral.Modules.Prefire;
using PracCentral.Services.Engine;
using PracCentral.Services.Storage;
using PracCentral.Services.Threading;
using PracCentral.Services.Validation;

namespace PracCentral.Tests;

public sealed class ModeConfigurationTests
{
    [Fact]
    public void AimModule_Load_UsesConfiguredFlags()
    {
        var module = new AimModule();
        module.Load(BuildContext(new PluginConfig
        {
            Aim = new AimModeConfig
            {
                HeadshotOnlyEnabled = false,
                BulletEconomyEnabled = false,
            },
        }));

        Assert.False(module.HeadshotOnlyEnabled);
        Assert.False(module.BulletEconomyEnabled);
    }

    [Fact]
    public void PrefireModule_LoadAndUnload_RespectsBotKickFlags()
    {
        var bridge = new FakeServerBridge();
        var module = new PrefireModule();
        var context = BuildContext(new PluginConfig
        {
            Prefire = new PrefireModeConfig
            {
                KickBotsOnLoad = false,
                KickBotsOnUnload = false,
                DataDirectory = "data\\prefire",
            },
        }, bridge);

        module.Load(context);
        module.Unload(context);

        Assert.Empty(bridge.Commands);
    }

    [Fact]
    public void GrenadeModule_RecordThrow_WhenSavingDisabled_DoesNotStoreThrow()
    {
        var module = new GrenadeModule();
        module.Load(BuildContext(new PluginConfig
        {
            Grenade = new GrenadeModeConfig
            {
                SaveLastThrow = false,
            },
        }));

        module.RecordThrow(new GrenadeThrowSnapshot(1, 2, 3, 4, 5, 6));

        Assert.Null(module.GetLastThrow());
    }

    private static ModuleContext BuildContext(PluginConfig config, FakeServerBridge? serverBridge = null)
    {
        return new ModuleContext(
            new MainThreadDispatcher(static callback => callback()),
            serverBridge ?? new FakeServerBridge(),
            new JsonStorageService(),
            new InputSanitizer(),
            new EventSubscriptionRegistry(),
            new NullPracLogger(),
            config);
    }

    private sealed class FakeServerBridge : IServerBridge
    {
        public List<string> Commands { get; } = new();

        public void ExecuteCommand(string command)
        {
            Commands.Add(command);
        }
    }
}
