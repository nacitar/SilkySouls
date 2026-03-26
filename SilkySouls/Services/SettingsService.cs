using SilkySouls.Interfaces;
using SilkySouls.memory;
using SilkySouls.Memory;

namespace SilkySouls.Services
{
    public class SettingsService(IMemoryService memoryService) : ISettingsService
    {
        public void Quitout()
        {
            var quitoutPtr =
                memoryService.FollowPointers(memoryService.Read<nint>(Offsets.MenuMan.Base), [
                    Offsets.MenuMan.Quitout
                ], false);
            memoryService.Write(quitoutPtr, (byte)2);
        }

        public void ToggleFastQuitout(int value)
        {
            memoryService.Write(Offsets.Patches.Quitout, (byte)value);
        }
        
    }
}