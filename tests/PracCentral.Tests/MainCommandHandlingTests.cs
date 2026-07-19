using PracCentral.Config;
using PracCentral.Core;
using PracCentral.Services.Engine;
using PracCentral.Services.Storage;
using PracCentral.Services.Threading;

namespace PracCentral.Tests;

public sealed class MainCommandHandlingTests : IDisposable
{
    private readonly string _tempDirectory;

    public MainCommandHandlingTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "PracCentralTests", Guid.NewGuid().ToString("N"));
    }

    [Fact]
    public async Task HandleCommand_WhenAliasExists_TransitionsToMappedMode()
    {
        var aliasPath = Path.Combine(_tempDirectory, Constants.ConfigDirectoryName, Constants.CommandAliasFileName);
        var jsonStorage = new JsonStorageService();
        await jsonStorage.WriteAsync(aliasPath, new Dictionary<string, string>
        {
            [".gr"] = ".grenade",
        });

        var app = new Main(
            new MainThreadDispatcher(static callback => callback()),
            new FakeServerBridge(),
            new FakeConVarAccessor(),
            _tempDirectory);

        var handled = app.HandleCommand(".gr");

        Assert.True(handled);
        Assert.Equal(PracMode.Grenade, app.StateManager.CurrentMode);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    private sealed class FakeServerBridge : IServerBridge
    {
        public void ExecuteCommand(string command)
        {
        }
    }

    private sealed class FakeConVarAccessor : IConVarAccessor
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal)
        {
            ["mp_freezetime"] = "5",
            ["mp_roundtime"] = "1.92",
            ["sv_infinite_ammo"] = "0",
        };

        public string GetValue(string conVarName)
        {
            return _values[conVarName];
        }

        public void SetValue(string conVarName, string value)
        {
            _values[conVarName] = value;
        }
    }
}
