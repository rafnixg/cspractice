using PracCentral.Config;
using PracCentral.Models.Json;
using PracCentral.Services.Storage;

namespace PracCentral.Tests;

public sealed class ConfigServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly JsonStorageService _storage = new();

    public ConfigServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "PracCentralTests", Guid.NewGuid().ToString("N"));
    }

    [Fact]
    public async Task LoadOrCreateAsync_WhenConfigDoesNotExist_CreatesDefaultConfig()
    {
        var service = new ConfigService(_storage);

        var config = await service.LoadOrCreateAsync(_tempDirectory);
        var configPath = Path.Combine(_tempDirectory, Constants.ConfigDirectoryName, Constants.PluginConfigFileName);

        Assert.True(File.Exists(configPath));
        Assert.Equal("1.0", config.Version);
        Assert.True(config.Aim.HeadshotOnlyEnabled);
        Assert.True(config.Aim.BulletEconomyEnabled);
    }

    [Fact]
    public async Task LoadOrCreateAsync_WhenConfigExists_LoadsStoredConfig()
    {
        var configPath = Path.Combine(_tempDirectory, Constants.ConfigDirectoryName, Constants.PluginConfigFileName);
        await _storage.WriteAsync(configPath, new PluginConfig
        {
            Version = "2.0-test",
            Aim = new AimModeConfig
            {
                HeadshotOnlyEnabled = false,
                BulletEconomyEnabled = false,
            },
            Prefire = new PrefireModeConfig
            {
                KickBotsOnLoad = false,
                KickBotsOnUnload = false,
                DataDirectory = "custom",
            },
            Grenade = new GrenadeModeConfig
            {
                SaveLastThrow = false,
            },
        });

        var service = new ConfigService(_storage);
        var config = await service.LoadOrCreateAsync(_tempDirectory);

        Assert.Equal("2.0-test", config.Version);
        Assert.False(config.Aim.HeadshotOnlyEnabled);
        Assert.False(config.Aim.BulletEconomyEnabled);
        Assert.False(config.Prefire.KickBotsOnLoad);
        Assert.False(config.Prefire.KickBotsOnUnload);
        Assert.Equal("custom", config.Prefire.DataDirectory);
        Assert.False(config.Grenade.SaveLastThrow);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}
