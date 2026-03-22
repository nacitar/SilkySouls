// 

using System;
using SilkySouls.Enums;
using SilkySouls.GameIds;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services;

public class EzStateService(IMemoryService memoryService) : IEzStateService
{
    public void ExecuteTalkCommand(EzState.TalkCommand command)
    {
        if (command == null) return;
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.EzStateTalkCode;
        var paramsLoc = CodeCaveOffsets.Base + CodeCaveOffsets.EzStateTalkParams;

        for (int i = 0; i < command.Params.Length; i++)
        {
            memoryService.Write(paramsLoc + i * 4, command.Params[i]);
        }

        var bytes = AsmLoader.GetAsmBytes(AsmScript.ExecuteTalkEvent);
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x16, Functions.ExternalEventTempCtor, 5, 0x16 + 1),
            (code + 0x4C, paramsLoc, 7, 0x4C + 3),
            (code + 0x72, Functions.SetExternalEventTempParam, 5, 0x72 + 1),
            (code + 0x8B, Functions.ExecuteTalkEvent, 5, 0x8B + 1)
        ]);

        AsmHelper.WriteImmediateDwords(bytes, [
            (command.CommandId, 0x11 + 1),
            (command.Params.Length, 0x3F + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        memoryService.RunThread(code);
    }
}