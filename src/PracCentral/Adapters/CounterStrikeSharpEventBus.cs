using CounterStrikeSharp.API;
using PracCentral.Services.Engine;

namespace PracCentral.Adapters;

public sealed class CounterStrikeSharpEventBus : IDisposable
{
    private readonly Dictionary<Type, List<Delegate>> _subscriptions = [];

    public IDisposable Subscribe<TEventData>(Action<TEventData> handler) where TEventData : class
    {
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(TEventData);
        if (!_subscriptions.ContainsKey(eventType))
        {
            _subscriptions[eventType] = [];
        }

        _subscriptions[eventType].Add(handler);

        return new UnsubscribeToken(() =>
        {
            if (_subscriptions.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
            }
        });
    }

    public void RaiseEvent<TEventData>(TEventData eventData) where TEventData : class
    {
        var eventType = typeof(TEventData);
        if (_subscriptions.TryGetValue(eventType, out var handlers))
        {
            foreach (var handler in handlers.ToList())
            {
                ((Action<TEventData>)handler).Invoke(eventData);
            }
        }
    }

    public void Dispose()
    {
        _subscriptions.Clear();
    }

    private sealed class UnsubscribeToken(Action unsubscribe) : IDisposable
    {
        public void Dispose()
        {
            unsubscribe?.Invoke();
        }
    }
}
