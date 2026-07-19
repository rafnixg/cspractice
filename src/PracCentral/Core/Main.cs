using PracCentral.Config;
using PracCentral.Core.Contracts;
using PracCentral.Infrastructure.Logging;
using PracCentral.Modules.Aim;
using PracCentral.Modules.Grenade;
using PracCentral.Modules.Prefire;
using PracCentral.Services.Engine;
using PracCentral.Services.Storage;
using PracCentral.Services.Threading;
using PracCentral.Services.Validation;

namespace PracCentral.Core;

public sealed class Main
{
    private readonly Dictionary<string, PracMode> _commandToMode = new(StringComparer.OrdinalIgnoreCase)
    {
        [".grenade"] = PracMode.Grenade,
        [".prefire"] = PracMode.Prefire,
        [".aim"] = PracMode.Aim,
        [".idle"] = PracMode.Idle,
    };

    public Main(
        IMainThreadDispatcher mainThreadDispatcher,
        IServerBridge serverBridge,
        IConVarAccessor conVarAccessor,
        IPracLogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(mainThreadDispatcher);
        ArgumentNullException.ThrowIfNull(serverBridge);
        ArgumentNullException.ThrowIfNull(conVarAccessor);

        logger ??= new NullPracLogger();

        var eventSubscriptionRegistry = new EventSubscriptionRegistry();
        var moduleContext = new ModuleContext(
            mainThreadDispatcher,
            serverBridge,
            new JsonStorageService(),
            new InputSanitizer(),
            eventSubscriptionRegistry,
            logger);

        var moduleByMode = new Dictionary<PracMode, IPracModule>
        {
            [PracMode.Grenade] = new GrenadeModule(),
            [PracMode.Prefire] = new PrefireModule(),
            [PracMode.Aim] = new AimModule(),
        };

        StateManager = new StateManager(
            moduleByMode,
            new DefaultStateTransitionGuard(),
            new ConVarSnapshotService(conVarAccessor, ConVarDefaults.Values.Keys),
            eventSubscriptionRegistry,
            moduleContext,
            logger);

        StateManager.Initialize();
    }

    public StateManager StateManager { get; }

    public bool HandleCommand(string commandText)
    {
        if (string.Equals(commandText, "!prac", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!_commandToMode.TryGetValue(commandText, out var mode))
        {
            return false;
        }

        StateManager.TransitionTo(mode);
        return true;
    }
}
