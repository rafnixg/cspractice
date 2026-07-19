namespace PracCentral.Core.Contracts;

public interface IPracModule : IDisposable
{
    string Name { get; }
    void Load(ModuleContext context);
    void Unload(ModuleContext context);
}
