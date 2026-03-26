using System;
using System.Windows.Threading;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services;

public class DebugDrawService : IDebugDrawService
{
    private readonly IMemoryService _memoryService;
    private readonly HookManager _hookManager;
    private readonly IStateService _stateService;
    private bool _isCtorHookInstalled;

    public DebugDrawService(IMemoryService memoryService, HookManager hookManager, IStateService stateService)
    {
        _memoryService = memoryService;
        _hookManager = hookManager;
        _stateService = stateService;

        stateService.Subscribe(State.Detached, () => _isCtorHookInstalled = false);
    }

    private void EnsureDebugDrawEnabled()
    {
        if (_isCtorHookInstalled) return;

        var code = CodeCaveOffsets.Base + CodeCaveOffsets.EnableDraw;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EnableDraw);
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, HgDraw.Base, 7, 3),
            (code + 0x19, Functions.SetTag, 5, 0x19 + 1),
            (code + 0x4D, Functions.FinalizeEntry, 5, 0x4D + 1),
            (code + 0x5D, Functions.SetTag, 5, 0x5D + 1),
            (code + 0x7D, Functions.FinalizeEntry, 5, 0x7D + 1),
            (code + 0x8A, Hooks.Draw + 8, 5, 0x8A + 1),
        ]);

        _memoryService.WriteBytes(code, bytes);
        _hookManager.InstallHook(code, Hooks.Draw, [0x44, 0x8B, 0xC6, 0xBA, 0x16, 0x00, 0x00, 0x00]);
        _isCtorHookInstalled = true;

        if (_stateService.IsLoaded())
        {
            InstallLoadedHook();
        }
    }

    private void InstallLoadedHook()
    {
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.LoadedEnableDraw;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.LoadedEnableDraw);
        var enableFlag = CodeCaveOffsets.Base + CodeCaveOffsets.EnableFlag;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, enableFlag, 7, 2),
            (code + 0xD, enableFlag, 7, 0xD + 2),
            (code + 0x2F, HgDraw.Base, 7, 0x2F + 3),
            (code + 0x15A, Functions.AllocateMemory, 5, 0x15A + 1),
            (code + 0x17C, Functions.AllocateMemory, 5, 0x17C + 1),
            (code + 0x189, BeginTargetSceneVtable, 7, 0x189 + 3),
            (code + 0x19E, EndTargetSceneVtable, 7, 0x19E + 3),
            (code + 0x1FC, enableFlag, 7, 0x1FC + 2),
            (code + 0x223, Hooks.HgDrawCommandExecutor + 5, 5, 0x223 + 1)
        ]);

        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (HGDrawPlanEntityVtable, 0xD3 + 2),
        ]);
        
        _memoryService.Write(enableFlag, true);

        _memoryService.WriteBytes(code, bytes);
        _hookManager.InstallHook(code, Hooks.HgDrawCommandExecutor, [0x31, 0xF6, 0x45, 0x31, 0xED]);

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        timer.Tick += (_, _) =>
        {
            _hookManager.UninstallHook(code);
            timer.Stop();
        };
        timer.Start();
    }

    public void ToggleDrawHitbox(bool isEnabled)
    {
        if (isEnabled) EnsureDebugDrawEnabled();
        var ptr = _memoryService.Read<nint>(DamageManager.Base) + DamageManager.HitboxFlag;
        _memoryService.Write(ptr, isEnabled);
    }

    public void ToggleDrawSoundView(bool isEnabled)
    {
        if (isEnabled) EnsureDebugDrawEnabled();
        _memoryService.Write(Patches.DrawSoundView, isEnabled);
    }

    public void ToggleDrawEvents(bool isEnabled)
    {
        if (isEnabled) EnsureDebugDrawEnabled();
        _memoryService.Write(Patches.DrawEvent, isEnabled);
    }
}
