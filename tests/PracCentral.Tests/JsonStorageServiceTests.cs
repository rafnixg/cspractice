using PracCentral.Services.Storage;

namespace PracCentral.Tests;

public sealed class JsonStorageServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly JsonStorageService _storage = new();

    public JsonStorageServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "PracCentralTests", Guid.NewGuid().ToString("N"));
    }

    [Fact]
    public async Task WriteAsync_AndReadAsync_RoundtripPayload()
    {
        var filePath = Path.Combine(_tempDirectory, "roundtrip.json");
        var payload = new SamplePayload("de_mirage", 3);

        await _storage.WriteAsync(filePath, payload);
        var reloaded = await _storage.ReadAsync<SamplePayload>(filePath);

        Assert.Equal(payload, reloaded);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    private sealed record SamplePayload(string MapName, int Nodes);
}
