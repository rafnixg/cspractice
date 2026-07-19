using PracCentral.Services.Validation;

namespace PracCentral.Tests;

public class InputSanitizerTests
{
    private readonly InputSanitizer _sanitizer = new();

    [Theory]
    [InlineData("lineupA")]
    [InlineData("lineup_01")]
    [InlineData("lineup-01")]
    public void IsSafeIdentifier_ReturnsTrue_ForValidNames(string value)
    {
        Assert.True(_sanitizer.IsSafeIdentifier(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("../escape")]
    [InlineData("folder\\name")]
    [InlineData("name with spaces")]
    public void IsSafeIdentifier_ReturnsFalse_ForUnsafeNames(string value)
    {
        Assert.False(_sanitizer.IsSafeIdentifier(value));
    }
}
