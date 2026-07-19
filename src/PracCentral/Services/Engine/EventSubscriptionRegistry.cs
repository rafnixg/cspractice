namespace PracCentral.Services.Engine;

public sealed class EventSubscriptionRegistry : IDisposable
{
    private readonly List<IDisposable> _subscriptions = [];

    public void Register(IDisposable subscription)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        _subscriptions.Add(subscription);
    }

    public void Dispose()
    {
        DisposeAll();
    }

    public void DisposeAll()
    {
        List<Exception>? errors = null;

        for (var index = _subscriptions.Count - 1; index >= 0; index--)
        {
            try
            {
                _subscriptions[index].Dispose();
            }
            catch (Exception exception)
            {
                errors ??= [];
                errors.Add(exception);
            }
        }

        _subscriptions.Clear();
        if (errors is { Count: > 0 })
        {
            throw new AggregateException("One or more event subscriptions failed to dispose.", errors);
        }
    }
}
