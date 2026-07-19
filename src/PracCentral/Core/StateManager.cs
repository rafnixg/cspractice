using PracCentral.Core.Contracts;
using PracCentral.Infrastructure.Logging;
using PracCentral.Services.Engine;

namespace PracCentral.Core;

public sealed class StateManager
{
    private readonly IReadOnlyDictionary<PracMode, IPracModule> _moduleByMode;
    private readonly IStateTransitionGuard _stateTransitionGuard;
    private readonly ConVarSnapshotService _conVarSnapshotService;
    private readonly EventSubscriptionRegistry _eventSubscriptionRegistry;
    private readonly ModuleContext _moduleContext;
    private readonly IPracLogger _logger;
    private IPracModule? _activeModule;

    public StateManager(
        IReadOnlyDictionary<PracMode, IPracModule> moduleByMode,
        IStateTransitionGuard stateTransitionGuard,
        ConVarSnapshotService conVarSnapshotService,
        EventSubscriptionRegistry eventSubscriptionRegistry,
        ModuleContext moduleContext,
        IPracLogger logger)
    {
        _moduleByMode = moduleByMode ?? throw new ArgumentNullException(nameof(moduleByMode));
        _stateTransitionGuard = stateTransitionGuard ?? throw new ArgumentNullException(nameof(stateTransitionGuard));
        _conVarSnapshotService = conVarSnapshotService ?? throw new ArgumentNullException(nameof(conVarSnapshotService));
        _eventSubscriptionRegistry = eventSubscriptionRegistry ?? throw new ArgumentNullException(nameof(eventSubscriptionRegistry));
        _moduleContext = moduleContext ?? throw new ArgumentNullException(nameof(moduleContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public PracMode CurrentMode { get; private set; } = PracMode.Idle;

    public void Initialize()
    {
        _conVarSnapshotService.CaptureDefaults();
    }

    public void TransitionTo(PracMode nextMode)
    {
        if (CurrentMode == nextMode)
        {
            return;
        }

        _stateTransitionGuard.ValidateTransition(CurrentMode, nextMode);
        var previousMode = CurrentMode;
        CurrentMode = PracMode.Transition;
        _logger.Info($"Transitioning from {previousMode} to {nextMode}.");

        _activeModule?.Unload(_moduleContext);
        _activeModule?.Dispose();
        _activeModule = null;

        _eventSubscriptionRegistry.DisposeAll();
        _conVarSnapshotService.RestoreDefaults();

        if (nextMode == PracMode.Idle)
        {
            CurrentMode = PracMode.Idle;
            _logger.Info("System is now in idle mode.");
            return;
        }

        if (!_moduleByMode.TryGetValue(nextMode, out var module))
        {
            throw new KeyNotFoundException($"No module is registered for mode {nextMode}.");
        }

        module.Load(_moduleContext);
        _activeModule = module;
        CurrentMode = nextMode;
        _logger.Info($"Module {module.Name} loaded.");
    }
}
