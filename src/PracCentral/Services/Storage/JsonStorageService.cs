using System.Text.Json;

namespace PracCentral.Services.Storage;

public sealed class JsonStorageService : IJsonStorageService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public async Task<T> ReadAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        await using var file = File.OpenRead(path);
        var model = await JsonSerializer.DeserializeAsync<T>(file, SerializerOptions, cancellationToken);
        return model ?? throw new InvalidOperationException($"Unable to deserialize JSON file: {path}");
    }

    public async Task WriteAsync<T>(string path, T payload, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(payload);

        var directory = Path.GetDirectoryName(path);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException($"Cannot resolve directory for path: {path}");
        }

        Directory.CreateDirectory(directory);
        await using var file = File.Create(path);
        await JsonSerializer.SerializeAsync(file, payload, SerializerOptions, cancellationToken);
    }
}
