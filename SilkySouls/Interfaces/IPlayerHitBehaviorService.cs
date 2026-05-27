namespace SilkySouls.Interfaces;

public interface IPlayerHitBehaviorService
{
    bool IsNegateNonFatalHitDamageEnabled { get; }
    bool IsHealEnemiesOnPlayerHitEnabled { get; }
    void SetNegateNonFatalHitDamageEnabled(bool isEnabled);
    void SetHealEnemiesOnPlayerHitEnabled(bool isEnabled);
}
