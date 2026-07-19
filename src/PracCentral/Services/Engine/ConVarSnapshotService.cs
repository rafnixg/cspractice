namespace PracCentral.Services.Engine;

public sealed class ConVarSnapshotService
{
    private readonly IConVarAccessor _conVarAccessor;
    private readonly IReadOnlyCollection<string> _trackedConVars;
    private readonly Dictionary<string, string> _defaults = new(StringComparer.Ordinal);

    public ConVarSnapshotService(IConVarAccessor conVarAccessor, IEnumerable<string> trackedConVars)
    {
        _conVarAccessor = conVarAccessor ?? throw new ArgumentNullException(nameof(conVarAccessor));
        _trackedConVars = trackedConVars?.ToArray() ?? throw new ArgumentNullException(nameof(trackedConVars));
    }

    public void CaptureDefaults()
    {
        foreach (var conVar in _trackedConVars)
        {
            _defaults[conVar] = _conVarAccessor.GetValue(conVar);
        }
    }

    public void RestoreDefaults()
    {
        if (_defaults.Count == 0)
        {
            throw new InvalidOperationException("ConVar defaults were not captured before restore.");
        }

        foreach (var pair in _defaults)
        {
            _conVarAccessor.SetValue(pair.Key, pair.Value);
        }
    }
}
