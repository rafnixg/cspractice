namespace PracCentral.Services.Engine.Events;

public sealed record GrenadeThrowEventData(
    int ThrowerId,
    double OriginX,
    double OriginY,
    double OriginZ,
    double VelocityX,
    double VelocityY,
    double VelocityZ,
    string GrenadeType);

public sealed record PlayerDamageEventData(
    int AttackerId,
    int VictimId,
    int Hitgroup,
    float Damage);

public sealed record PlayerDeathEventData(
    int AttackerId,
    int VictimId,
    string Weapon);
