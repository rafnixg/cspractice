namespace PracCentral.Config;

public static class ConVarDefaults
{
    public static readonly IReadOnlyDictionary<string, string> Values = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["mp_freezetime"] = "5",
        ["mp_roundtime"] = "1.92",
        ["sv_infinite_ammo"] = "0",
    };
}
