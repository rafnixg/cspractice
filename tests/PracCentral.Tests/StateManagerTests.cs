using PracCentral.Core;
using PracCentral.Core.Contracts;
using PracCentral.Infrastructure.Logging;
using PracCentral.Models.Json;
using PracCentral.Services.Engine;
using PracCentral.Services.Storage;
using PracCentral.Services.Threading;
using PracCentral.Services.Validation;

namespace PracCentral.Tests;

public sealed class StateManagerTests
{
    [Fact]
    public void TransitionTo_ChangesModeAndUnloadsPreviousModule()
    {
        var grenade = new FakeModule("grenade");
        var aim = new FakeModule("aim");
        var (stateManager, _) = BuildStateManager(grenade, aim);
        stateManager.Initialize();

        stateManager.TransitionTo(PracMode.Grenade);
        stateManager.TransitionTo(PracMode.Aim);

        Assert.Equal(PracMode.Aim, stateManager.CurrentMode);
        Assert.Equal(1, grenade.LoadCalls);
        Assert.Equal(1, grenade.UnloadCalls);
        Assert.Equal(1, aim.LoadCalls);
    }

    [Fact]
    public void TransitionTo_Idle_UnloadsActiveModule()
    {
        var grenade = new FakeModule("grenade");
        var aim = new FakeModule("aim");
        var (stateManager, _) = BuildStateManager(grenade, aim);
        stateManager.Initialize();

        stateManager.TransitionTo(PracMode.Grenade);
        stateManager.TransitionTo(PracMode.Idle);

        Assert.Equal(PracMode.Idle, stateManager.CurrentMode);
        Assert.Equal(1, grenade.UnloadCalls);
    }

    private static (StateManager manager, FakeConVarAccessor accessor) BuildStateManager(FakeModule grenade, FakeModule aim)
    {
        var accessor = new FakeConVarAccessor(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["mp_freezetime"] = "5",
            ["mp_roundtime"] = "1.92",
            ["sv_infinite_ammo"] = "0",
        });

        var registry = new EventSubscriptionRegistry();
        var context = new ModuleContext(
            new MainThreadDispatcher(static callback => callback()),
            new FakeServerBridge(),
            "D:\\projectos\\cspractice",
            new JsonStorageService(),
            new InputSanitizer(),
            registry,
            new NullPracLogger(),
            new PluginConfig());

        var modules = new Dictionary<PracMode, IPracModule>
        {
            [PracMode.Grenade] = grenade,
            [PracMode.Aim] = aim,
        };

        var manager = new StateManager(
            modules,
            new DefaultStateTransitionGuard(),
            new ConVarSnapshotService(accessor, accessor.Keys),
            registry,
            context,
            new NullPracLogger());

        return (manager, accessor);
    }

    private sealed class FakeModule(string moduleName) : IPracModule
    {
        public int LoadCalls { get; private set; }
        public int UnloadCalls { get; private set; }
        public string Name { get; } = moduleName;

        public void Dispose()
        {
        }

        public void Load(ModuleContext context)
        {
            LoadCalls++;
        }

        public void Unload(ModuleContext context)
        {
            UnloadCalls++;
        }
    }

    private sealed class FakeServerBridge : IServerBridge
    {
        public void ExecuteCommand(string command)
        {
        }
    }

    private sealed class FakeConVarAccessor(Dictionary<string, string> values) : IConVarAccessor
    {
        public IEnumerable<string> Keys => values.Keys;

        public string GetValue(string conVarName)
        {
            return values[conVarName];
        }

        public void SetValue(string conVarName, string value)
        {
            values[conVarName] = value;
        }
    }
}
