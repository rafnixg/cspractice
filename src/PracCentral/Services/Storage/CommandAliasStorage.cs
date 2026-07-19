using PracCentral.Config;
using PracCentral.Services.Validation;

namespace PracCentral.Services.Storage;

public sealed class CommandAliasStorage
{
    private readonly IJsonStorageService _jsonStorageService;
    private readonly IInputSanitizer _inputSanitizer;

    public CommandAliasStorage(IJsonStorageService jsonStorageService, IInputSanitizer inputSanitizer)
    {
        _jsonStorageService = jsonStorageService ?? throw new ArgumentNullException(nameof(jsonStorageService));
        _inputSanitizer = inputSanitizer ?? throw new ArgumentNullException(nameof(inputSanitizer));
    }

    public async Task<IReadOnlyDictionary<string, string>> LoadOrCreateAsync(
        string moduleDirectoryPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleDirectoryPath);

        var filePath = Path.Combine(moduleDirectoryPath, Constants.ConfigDirectoryName, Constants.CommandAliasFileName);
        if (!File.Exists(filePath))
        {
            var emptyAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            await _jsonStorageService.WriteAsync(filePath, emptyAliases, cancellationToken);
            return emptyAliases;
        }

        var aliases = await _jsonStorageService.ReadAsync<Dictionary<string, string>>(filePath, cancellationToken);
        var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (alias, command) in aliases)
        {
            ValidateCommandToken(alias, nameof(alias));
            ValidateCommandToken(command, nameof(command));
            normalized[alias.Trim().ToLowerInvariant()] = command.Trim().ToLowerInvariant();
        }

        return normalized;
    }

    private void ValidateCommandToken(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Command alias {paramName} cannot be empty.");
        }

        if (value.Length > 64)
        {
            throw new InvalidOperationException($"Command alias {paramName} cannot be longer than 64 characters.");
        }

        if (!value.StartsWith('.') && !value.StartsWith('!'))
        {
            throw new InvalidOperationException($"Command alias {paramName} must start with '.' or '!'.");
        }

        var token = value[1..];
        if (!_inputSanitizer.IsSafeIdentifier(token))
        {
            throw new InvalidOperationException($"Command alias {paramName} contains unsupported characters.");
        }
    }
}
