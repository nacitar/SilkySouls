// 

using System.Numerics;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Interfaces;

public interface IPlayerService
{
    int GetHp();
    int GetMaxHp();
    void SetHp(int hp);
    void SetRtsr();
    void SetMaxHp();
    int GetSp();
    void SetSp(int sp);
    Vector3 GetPosition();
    void SavePosition(int index);
    void RestorePositon(int index);
    int GetNewGame();
    void SetNewGame(int newGame);
    float GetSpeed();
    void SetSpeed(float speed);
    void ToggleChrDebugFlag(int offset, bool isEnabled);
    void ToggleNoDamage(bool isEnabled);
    void ToggleInfiniteStamina(bool isEnabled);
    void ToggleNoGoodsConsume(bool isEnabled);
    void ToggleInfinitePoise(bool isEnabled);
    void ToggleInfiniteDurability(bool isEnabled);
    void ToggleNoRoll(bool isEnabled);
    void RestoreSpellCasts();
    void GiveSouls();
    int GetPlayerStat(GameDataMan.PlayerGameData stat);
    void SetPlayerStat(GameDataMan.PlayerGameData statType, int newValue);
    void BreakWeapon(int slotOffset);
}