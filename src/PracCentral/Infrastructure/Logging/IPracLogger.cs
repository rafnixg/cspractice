namespace PracCentral.Infrastructure.Logging;

public interface IPracLogger
{
    void Info(string message);
    void Warn(string message);
    void Error(string message);
}
