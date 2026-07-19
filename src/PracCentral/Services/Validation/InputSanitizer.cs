using System.Text.RegularExpressions;
using PracCentral.Config;

namespace PracCentral.Services.Validation;

public sealed partial class InputSanitizer : IInputSanitizer
{
    public bool IsSafeIdentifier(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        return SafeIdentifierRegex().IsMatch(input);
    }

    public string EnsureSafeIdentifier(string input)
    {
        if (!IsSafeIdentifier(input))
        {
            throw new ArgumentException("Identifier contains unsupported characters.", nameof(input));
        }

        return input;
    }

    [GeneratedRegex(Constants.SaveNamePattern, RegexOptions.CultureInvariant)]
    private static partial Regex SafeIdentifierRegex();
}
