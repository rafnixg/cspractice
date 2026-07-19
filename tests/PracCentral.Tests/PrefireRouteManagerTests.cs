using PracCentral.Models.Json;
using PracCentral.Modules.Prefire;
using PracCentral.Services.Storage;
using PracCentral.Services.Validation;

namespace PracCentral.Tests;

public sealed class PrefireRouteManagerTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly PrefireRouteManager _manager;

    public PrefireRouteManagerTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "PracCentralTests", Guid.NewGuid().ToString("N"));
        _manager = new PrefireRouteManager(new JsonStorageService(), new InputSanitizer());
    }

    [Fact]
    public async Task LoadOrCreateAsync_WhenMissing_CreatesFile()
    {
        var config = await _manager.LoadOrCreateAsync("de_mirage", _tempDirectory);

        Assert.Equal("de_mirage", config.MapName);
        Assert.True(File.Exists(Path.Combine(_tempDirectory, "de_mirage.json")));
    }

    [Fact]
    public async Task SaveRouteAsync_ThenBuildSpawnPlan_ReturnsOrderedInstructions()
    {
        var route = new PrefireRouteDefinition
        {
            Name = "route_a",
            StartingPoint = new PrefireRouteStartPoint
            {
                Position = new Vector3Dto(1, 2, 3),
                Angle = new Angle3Dto(0, 90, 0),
            },
            Nodes =
            [
                new PrefireRouteNodeDefinition
                {
                    Sequence = 2,
                    Wave = 1,
                    Position = new Vector3Dto(20, 0, 0),
                    Angle = new Angle3Dto(0, 90, 0),
                    WeaponId = "weapon_ak47",
                    BotDifficulty = 2,
                },
                new PrefireRouteNodeDefinition
                {
                    Sequence = 1,
                    Wave = 1,
                    Position = new Vector3Dto(10, 0, 0),
                    Angle = new Angle3Dto(0, 90, 0),
                    WeaponId = "weapon_ak47",
                    BotDifficulty = 2,
                },
            ],
        };

        await _manager.SaveRouteAsync("de_mirage", _tempDirectory, route);
        var loaded = await _manager.GetRouteAsync("de_mirage", _tempDirectory, "route_a");
        var plan = _manager.BuildSpawnPlan(loaded);

        Assert.Equal(2, plan.Count);
        Assert.Equal(1, plan[0].Sequence);
        Assert.Equal(2, plan[1].Sequence);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}
