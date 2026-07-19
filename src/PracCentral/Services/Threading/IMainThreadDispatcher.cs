namespace PracCentral.Services.Threading;

public interface IMainThreadDispatcher
{
    void Enqueue(Action callback);
}
