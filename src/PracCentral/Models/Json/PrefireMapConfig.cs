namespace PracCentral.Models.Json;

public sealed record PrefireMapConfig(string MapName, IReadOnlyList<PrefireNode> Nodes);

public sealed record PrefireNode(
    int Id,
    Vector3Dto VectorPos,
    Angle3Dto VectorAng,
    string WeaponId,
    int BotDifficulty);

public sealed record Vector3Dto(double X, double Y, double Z);

public sealed record Angle3Dto(double Pitch, double Yaw, double Roll);
