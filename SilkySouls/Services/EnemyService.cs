// 

using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services;

public class EnemyService(IMemoryService memoryService, HookManager hookManager) : IEnemyService
{
    public void ToggleEnemiesDebugFlag(int offset, bool isEnabled) =>
        memoryService.Write(DebugFlags.Base + offset, isEnabled);

    public void DisableFourKingsGenerator(bool isEnabled)
    {
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.DisableFourKingsGenerator;
        if (isEnabled)
        {
            var bytes = AsmLoader.GetAsmBytes(AsmScript.DisableFourKingsGenerator);
            AsmHelper.WriteRelativeOffset(bytes, code + 0x16, Hooks.FourKingsGenerator + 5, 5, 0x16 + 1);
            memoryService.WriteBytes(code, bytes);
            hookManager.InstallHook(code, Hooks.FourKingsGenerator, [0x48, 0x89, 0x6C, 0x24, 0x10]);
        }
        else
        {
            hookManager.UninstallHook(code);
        }
    }
}