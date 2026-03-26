// 

using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services;

public class UtilityService(IMemoryService memoryService, HookManager hookManager) : IUtilityService
{
    public const float DefaultNoClipSpeedScale = 0.2f;

    public void ShowMenu(int offset, int val) =>
        memoryService.Write(memoryService.Read<nint>(MenuMan.Base) + offset, val);

    public void ToggleDebugDraw(bool isEnabled)
    {
        throw new System.NotImplementedException();
    }
    

    public void ToggleNoClip(bool isEnabled)
    {
        var inAirTimerCode = CodeCaveOffsets.Base + CodeCaveOffsets.InAirTimer;
        var kbCode = CodeCaveOffsets.Base + CodeCaveOffsets.Kb;
        var rightTriggerCode = CodeCaveOffsets.Base + CodeCaveOffsets.TriggerR2;
        var leftTriggerCode = CodeCaveOffsets.Base + CodeCaveOffsets.TriggerL2;
        var updateCoordsCode = CodeCaveOffsets.Base + CodeCaveOffsets.UpdateCoords;

        var playerIns =
            memoryService.Read<nint>(memoryService.Read<nint>(WorldChrMan.Base) + WorldChrMan.PlayerIns);

        if (isEnabled)
        {
            WriteInAirTimer(inAirTimerCode);
            WriteKeyboardHook(kbCode);
            WriteRightTriggerCode(rightTriggerCode);
            WriteLeftTriggerCode(leftTriggerCode);
            WriteUpdateCoordsCode(updateCoordsCode);

            hookManager.InstallHook(inAirTimerCode, Hooks.InAirTimer, [0xF3, 0x0F, 0x58, 0x9B, 0xB0, 0x01, 0x00, 0x00]);
            hookManager.InstallHook(kbCode, Hooks.Keyboard, [0xC6, 0x43, 0xF0, 0x01, 0xC6, 0x00, 0x01]);
            hookManager.InstallHook(rightTriggerCode, Hooks.ControllerR2, [0x0F, 0xB6, 0x44, 0x24, 0x27]);
            hookManager.InstallHook(leftTriggerCode, Hooks.ControllerL2, [0x0F, 0xB6, 0x44, 0x24, 0x26]);
            hookManager.InstallHook(updateCoordsCode, Hooks.UpdateCoords, [0x0F, 0x29, 0x81, 0x20, 0x01, 0x00, 0x00]);

            memoryService.SetBitValue(playerIns + ChrIns.NoGravity.Offset, ChrIns.NoGravity.Bit, true);
        }
        else
        {
            hookManager.UninstallHook(inAirTimerCode);
            hookManager.UninstallHook(kbCode);
            hookManager.UninstallHook(rightTriggerCode);
            hookManager.UninstallHook(leftTriggerCode);
            hookManager.UninstallHook(updateCoordsCode);

            memoryService.SetBitValue(playerIns + ChrIns.NoGravity.Offset, ChrIns.NoGravity.Bit, false);
        }
    }

    public void WriteNoClipSpeed(float speedScale)
    {
        var ptr = CodeCaveOffsets.Base + CodeCaveOffsets.SpeedScale;
        memoryService.Write(ptr, DefaultNoClipSpeedScale * speedScale);
    }

    private void WriteInAirTimer(nint code)
    {
        var codeBytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_InAirTimer);
        AsmHelper.WriteRelativeOffsets(codeBytes, [
            (code + 0x9, WorldChrMan.Base, 7, 0x9 + 3),
            (code + 0x31, Hooks.InAirTimer + 8, 5, 0x31 + 1)
        ]);

        memoryService.WriteBytes(code, codeBytes);
    }

    private void WriteKeyboardHook(nint code)
    {
        var codeBytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_ZDirection_KB);
        var zDirection = CodeCaveOffsets.Base + CodeCaveOffsets.ZDirection;

        AsmHelper.WriteRelativeOffsets(codeBytes, [
            (code + 0x16, Hooks.Keyboard + 7, 5, 0x16 + 1),
            (code + 0x1b, zDirection, 7, 0x1B + 2),
            (code + 0x29, Hooks.Keyboard + 7, 5, 0x29 + 1),
            (code + 0x2E, zDirection, 7, 0x2E + 2),
            (code + 0x3C, Hooks.Keyboard + 7, 5, 0x3C + 1),
        ]);

        memoryService.WriteBytes(code, codeBytes);
    }

    private void WriteRightTriggerCode(nint code)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_ZDirection_R2);
        var zDirection = CodeCaveOffsets.Base + CodeCaveOffsets.ZDirection;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, Hooks.ControllerR2 + 5, 6, 0x7 + 2),
            (code + 0xD, zDirection, 7, 0xD + 2),
            (code + 0x16, Hooks.ControllerR2 + 5, 5, 0x16 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
    }

    private void WriteLeftTriggerCode(nint code)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_ZDirection_L2);
        var zDirection = CodeCaveOffsets.Base + CodeCaveOffsets.ZDirection;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, Hooks.ControllerL2 + 5, 6, 0x7 + 2),
            (code + 0xD, zDirection, 7, 0xD + 2),
            (code + 0x16, Hooks.ControllerL2 + 5, 5, 0x16 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
    }

    private void WriteUpdateCoordsCode(nint code)
    {
        var codeBytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_UpdateCoords);
        var zDirection = CodeCaveOffsets.Base + CodeCaveOffsets.ZDirection;
        var speedScale = CodeCaveOffsets.Base + CodeCaveOffsets.SpeedScale;

        AsmHelper.WriteRelativeOffsets(codeBytes, [
            (code + 0x1, WorldChrMan.Base, 7, 0x1 + 3),
            (code + 0x96, PadMan.Base, 7, 0x96 + 3),
            (code + 0xA1, DbgMapWalkPadVtable, 7, 0xA1 + 3),
            (code + 0xC0, Functions.GetYMovement, 5, 0xC0 + 1),
            (code + 0xCF, Functions.GetXMovement, 5, 0xCF + 1),
            (code + 0x103, FieldArea.Base, 7, 0x103 + 3),
            (code + 0x116, Functions.MatrixVectorProduct, 5, 0x116 + 1),
            (code + 0x152, speedScale, 9, 0x152 + 5),
            (code + 0x168, zDirection, 6, 0x168 + 2),
            (code + 0x192, zDirection, 7, 0x192 + 2),
            (code + 0x1C0, Hooks.UpdateCoords + 7, 5, 0x1C0 + 1)
        ]);
        memoryService.WriteBytes(code, codeBytes);
    }

    public void ToggleDeathCamera(bool isEnabled) =>
        memoryService.Write(memoryService.Read<nint>(WorldChrMan.Base) + (int)WorldChrMan.BaseOffsets.DeathCam,
            isEnabled ? (byte)1 : (byte)0);
}