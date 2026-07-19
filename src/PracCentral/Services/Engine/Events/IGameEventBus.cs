namespace PracCentral.Services.Engine.Events;

public interface IGameEventBus
{
    IDisposable Subscribe<TEventData>(Action<TEventData> handler) where TEventData : class;
}
