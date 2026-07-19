namespace PracCentral.Services.Storage;

public interface IJsonStorageService
{
    Task<T> ReadAsync<T>(string path, CancellationToken cancellationToken = default);
    Task WriteAsync<T>(string path, T payload, CancellationToken cancellationToken = default);
}
