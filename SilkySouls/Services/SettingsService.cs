using SilkySouls.Interfaces;
using SilkySouls.memory;
using SilkySouls.Memory;

namespace SilkySouls.Services
{
    public class SettingsService
    {
        private readonly IMemoryService _memoryService;

        public SettingsService(IMemoryService memoryService)
        {
            _memoryService = memoryService;
        }

        public void Quitout()
        {
            var quitoutPtr =
                _memoryService.FollowPointers(Offsets.MenuMan.Base, new[]
                {
                    (int)Offsets.MenuMan.MenuManData.Quitout
                }, false);
            _memoryService.Write(quitoutPtr, (byte)2);
        }

        public void ToggleFastQuitout(int value)
        {
            _memoryService.Write(Offsets.Patches.QuitoutPatch, (byte)value);
        }
        
    }
}