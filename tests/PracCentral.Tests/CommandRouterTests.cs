using PracCentral.Core;
using PracCentral.Services.Engine;

namespace PracCentral.Tests;

public sealed class CommandRouterTests
{
    [Fact]
    public void TryResolveMode_DirectCommand_ReturnsMode()
    {
        var router = BuildRouter(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

        var resolved = router.TryResolveMode(".aim", out var mode);

        Assert.True(resolved);
        Assert.Equal(PracMode.Aim, mode);
    }

    [Fact]
    public void TryResolveMode_AliasCommand_ReturnsMappedMode()
    {
        var router = BuildRouter(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".hs"] = ".aim",
        });

        var resolved = router.TryResolveMode(".hs", out var mode);

        Assert.True(resolved);
        Assert.Equal(PracMode.Aim, mode);
    }

    [Fact]
    public void TryResolveMode_UnknownCommand_ReturnsFalse()
    {
        var router = BuildRouter(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

        var resolved = router.TryResolveMode(".unknown", out var mode);

        Assert.False(resolved);
        Assert.Equal(PracMode.Idle, mode);
    }

    private static CommandRouter BuildRouter(IReadOnlyDictionary<string, string> aliases)
    {
        var commands = new Dictionary<string, PracMode>(StringComparer.OrdinalIgnoreCase)
        {
            [".grenade"] = PracMode.Grenade,
            [".prefire"] = PracMode.Prefire,
            [".aim"] = PracMode.Aim,
            [".idle"] = PracMode.Idle,
        };

        return new CommandRouter(commands, aliases);
    }
}
