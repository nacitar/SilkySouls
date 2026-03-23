// 

namespace SilkySouls.Interfaces;

public interface IEnemyService
{
    void ToggleEnemiesDebugFlag(int offset, bool isEnabled);
    void DisableFourKingsGenerator(bool isEnabled);
}