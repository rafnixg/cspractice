using PracCentral.Config;
using PracCentral.Services.Storage;
using PracCentral.Services.Validation;

namespace PracCentral.Tests;

public sealed class CommandAliasStorageTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly JsonStorageService _jsonStorageService = new();
    private readonly InputSanitizer _inputSanitizer = new();

    public CommandAliasStorageTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "PracCentralTests", Guid.NewGuid().ToString("N"));
    }

    [Fact]
    public async Task LoadOrCreateAsync_WhenFileIsMissing_CreatesEmptyAliasFile()
    {
        var storage = new CommandAliasStorage(_jsonStorageService, _inputSanitizer);

        var aliases = await storage.LoadOrCreateAsync(_tempDirectory);
        var aliasPath = Path.Combine(_tempDirectory, Constants.ConfigDirectoryName, Constants.CommandAliasFileName);

        Assert.Empty(aliases);
        Assert.True(File.Exists(aliasPath));
    }

    [Fact]
    public async Task LoadOrCreateAsync_WhenFileExists_ReturnsNormalizedAliases()
    {
        var aliasPath = Path.Combine(_tempDirectory, Constants.ConfigDirectoryName, Constants.CommandAliasFileName);
        await _jsonStorageService.WriteAsync(aliasPath, new Dictionary<string, string>
        {
            [".HS"] = ".AIM",
        });

        var storage = new CommandAliasStorage(_jsonStorageService, _inputSanitizer);
        var aliases = await storage.LoadOrCreateAsync(_tempDirectory);

        Assert.True(aliases.TryGetValue(".hs", out var mappedCommand));
        Assert.Equal(".aim", mappedCommand);
    }

    [Fact]
    public async Task LoadOrCreateAsync_WhenAliasContainsInvalidCharacters_Throws()
    {
        var aliasPath = Path.Combine(_tempDirectory, Constants.ConfigDirectoryName, Constants.CommandAliasFileName);
        await _jsonStorageService.WriteAsync(aliasPath, new Dictionary<string, string>
        {
            [".bad alias"] = ".aim",
        });

        var storage = new CommandAliasStorage(_jsonStorageService, _inputSanitizer);

        await Assert.ThrowsAsync<InvalidOperationException>(() => storage.LoadOrCreateAsync(_tempDirectory));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}
