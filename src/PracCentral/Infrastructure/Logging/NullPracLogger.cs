namespace PracCentral.Infrastructure.Logging;

public sealed class NullPracLogger : IPracLogger
{
    public void Error(string message)
    {
    }

    public void Info(string message)
    {
    }

    public void Warn(string message)
    {
    }
}
