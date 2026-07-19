namespace PracCentral.Services.Validation;

public interface IInputSanitizer
{
    bool IsSafeIdentifier(string input);
    string EnsureSafeIdentifier(string input);
}
