using System;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.memory;
using SilkySouls.Memory;
using SilkySouls.Utilities;

namespace SilkySouls.Services
{
    public class EnemyService(IMemoryService memoryService, HookManager hookManager)
    {
        
        public void ToggleAllNoDamage(int value)
        {
            var allNoDamagePtr = Offsets.DebugFlags.Base + Offsets.DebugFlags.AllNoDamage;
            memoryService.Write(allNoDamagePtr, value);

            var codeBlock = CodeCaveOffsets.Base + CodeCaveOffsets.AllNoDamage;
            if (value == 1)
            {
                nint origin = Offsets.Hooks.AllNoDamage;

                byte[] restoreHealthBytes = AsmLoader.GetAsmBytes(AsmScript.AllNoDamage);
                byte[] jumpBytes = BitConverter.GetBytes(origin + 7 - (codeBlock + 26));
                Array.Copy(jumpBytes, 0, restoreHealthBytes, 22, 4);
                memoryService.WriteBytes(codeBlock, restoreHealthBytes);
                hookManager.InstallHook(codeBlock, origin,
                    new byte[] { 0xF6, 0x81, 0x54, 0x01, 0x00, 0x00, 0x28 });
            }
            else
            {
                hookManager.UninstallHook(codeBlock);
            }
        }

        public void ToggleAllNoDeath(int value)
        {
            var allNoDeathPtr = Offsets.DebugFlags.Base + Offsets.DebugFlags.AllNoDeath;
            memoryService.Write(allNoDeathPtr, value);
        }

        public void ToggleAi(int value)
        {
            var disableAiPtr = Offsets.DebugFlags.Base + Offsets.DebugFlags.DisableAi;
            memoryService.Write(disableAiPtr, value);
        }

        public void Toggle4KingsTimer(bool is4KingsTimerStopped)
        {
            var patchLocation = Offsets.Patches.FourKingsPatch;
            if (is4KingsTimerStopped) memoryService.WriteBytes(patchLocation, new byte[] {0x90, 0x90, 0x90, 0x90, 0x90});
            else memoryService.WriteBytes(patchLocation, new byte[]{ 0xF3, 0x0F, 0x11, 0x47, 0x10 });
        }
    }
}