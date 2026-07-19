using PracCentral.Config;
using PracCentral.Models.Json;

namespace PracCentral.Services.Storage;

public sealed class ConfigService
{
    private readonly IJsonStorageService _jsonStorageService;

    public ConfigService(IJsonStorageService jsonStorageService)
    {
        _jsonStorageService = jsonStorageService ?? throw new ArgumentNullException(nameof(jsonStorageService));
    }

    public async Task<PluginConfig> LoadOrCreateAsync(string moduleDirectoryPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleDirectoryPath);

        var configPath = Path.Combine(moduleDirectoryPath, Constants.ConfigDirectoryName, Constants.PluginConfigFileName);
        if (File.Exists(configPath))
        {
            return await _jsonStorageService.ReadAsync<PluginConfig>(configPath, cancellationToken);
        }

        var defaultConfig = new PluginConfig();
        await _jsonStorageService.WriteAsync(configPath, defaultConfig, cancellationToken);
        return defaultConfig;
    }
}
