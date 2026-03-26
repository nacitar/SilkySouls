// 

namespace SilkySouls.Interfaces;

public interface IEventService
{
    void SetEvent(int eventId, bool setVal);
    bool GetEvent(int eventId);
    void ToggleDisableEvents(bool isEnabled);
    void OpenSensGate();
    void PlaceLordVessel();
    
}