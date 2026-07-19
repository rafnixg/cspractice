namespace PracCentral.Services.Engine;

public interface IConVarAccessor
{
    string GetValue(string conVarName);
    void SetValue(string conVarName, string value);
}
