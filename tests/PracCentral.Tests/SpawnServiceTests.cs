using PracCentral.Models.Json;
using PracCentral.Services.Engine;
using PracCentral.Services.Storage;
using PracCentral.Services.Validation;

namespace PracCentral.Tests;

public sealed class SpawnServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly SpawnService _service;
    private readonly JsonStorageService _storage = new();

    public SpawnServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "PracCentralTests", Guid.NewGuid().ToString("N"));
        _service = new SpawnService(_storage, new InputSanitizer());
    }

    [Fact]
    public async Task LoadMapSpawnsAsync_WhenCustomMissing_UsesDefaultSpawns()
    {
        var defaults = new[]
        {
            new SpawnPointDto("ct", new Vector3Dto(1, 2, 3), new Angle3Dto(0, 0, 0)),
        };

        var result = await _service.LoadMapSpawnsAsync("de_mirage", _tempDirectory, defaults);

        Assert.Equal(SpawnSource.Default, result.Source);
        Assert.Single(result.SpawnPoints);
    }

    [Fact]
    public async Task LoadMapSpawnsAsync_WhenCustomExists_UsesCustomSpawns()
    {
        var configPath = Path.Combine(_tempDirectory, "de_mirage.json");
        await _storage.WriteAsync(configPath, new SpawnMapConfig(
            "de_mirage",
            [
                new SpawnPointDto("t", new Vector3Dto(10, 20, 30), new Angle3Dto(0, 180, 0)),
            ]));

        var defaults = new[]
        {
            new SpawnPointDto("ct", new Vector3Dto(1, 2, 3), new Angle3Dto(0, 0, 0)),
        };

        var result = await _service.LoadMapSpawnsAsync("de_mirage", _tempDirectory, defaults);

        Assert.Equal(SpawnSource.Custom, result.Source);
        Assert.Equal("t", result.SpawnPoints[0].Team);
    }

    [Fact]
    public void SelectSafestSpawn_ChoosesCandidateWithLargestEnemyDistance()
    {
        var candidates = new[]
        {
            new SpawnPointDto("ct", new Vector3Dto(0, 0, 0), new Angle3Dto(0, 0, 0)),
            new SpawnPointDto("ct", new Vector3Dto(1000, 0, 0), new Angle3Dto(0, 0, 0)),
        };

        var enemies = new[]
        {
            new Vector3Dto(100, 0, 0),
        };

        var selected = _service.SelectSafestSpawn(candidates, enemies, 250);

        Assert.Equal(1000, selected.Position.X);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}
