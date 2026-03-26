namespace SilkySouls.Interfaces;

public interface IDebugDrawService
{
    void ToggleDrawHitbox(bool isEnabled);
    void ToggleDrawSoundView(bool isEnabled);
    void ToggleDrawEvents(bool isEnabled);
}
