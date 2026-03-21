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



    void RestoreSpellCasts();
    void GiveSouls();
    int GetPlayerStat(GameDataMan.PlayerGameData stat);
    void SetPlayerStat(GameDataMan.PlayerGameData statType, int newValue);
}