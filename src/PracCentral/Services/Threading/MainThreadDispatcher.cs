namespace PracCentral.Services.Threading;

public sealed class MainThreadDispatcher : IMainThreadDispatcher
{
    private readonly Action<Action> _nextFrame;

    public MainThreadDispatcher(Action<Action> nextFrame)
    {
        _nextFrame = nextFrame ?? throw new ArgumentNullException(nameof(nextFrame));
    }

    public void Enqueue(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _nextFrame(callback);
    }
}
