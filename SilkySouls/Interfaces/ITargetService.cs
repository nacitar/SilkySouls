// 

using System.Numerics;

namespace SilkySouls.Interfaces;

public interface ITargetService
{
    void ToggleTargetHook(bool isEnabled);
    nint GetChrIns();
    int GetHp();
    int GetMaxHp();
    void SetHp(int value);
    Vector3 GetPosition();
    bool IsAiDisabled();
    void ToggleAi(bool isDisableAiEnabled);
    bool IsNoDamageEnabled();
    void ToggleNoDamage(bool isNoDamageEnabled);
    float GetPoise();
    float GetMaxPoise();
    float GetPoiseTimer();
    int[] GetActs();
    int GetCurrentBleed();
    int GetMaxBleed();
    int GetCurrentPoison();
    int GetMaxPoison();
    int GetCurrentToxic();
    int GetMaxToxic();
    float GetSpeed();
    void SetSpeed(float speed);
    void RepeatAct(int index, int maxAct);
    void DisableRepeatAct();
    int GetCurrentRepeatEnemyId();
    int GetEnemyBattleId();
    int GetImmunitySpEffect();
}