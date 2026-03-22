// 

using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.GameIds.Emevd;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services;

public class EmevdService(IMemoryService memoryService) : IEmevdService
{
    public void ExecuteEmevdCommand(EmevdCommand command)
    {
        
        var args = CodeCaveOffsets.Base + CodeCaveOffsets.EmevdArgs;
        memoryService.WriteBytes(args, command.ParamData);
        
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ExecuteEmevd);
        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (EmkSystem.Base, 0x20 + 2),
            (Functions.EmkEventInsCtor, 0x53 + 2),
            (args, 0x81 + 2),
            (EmkSystem.Base, 0x95 + 2),
            (Functions.ExecuteEmevdCommand, 0xAC + 2)
        ]);
        
        AsmHelper.WriteImmediateDwords(bytes, [
            (command.GroupId, 0x66 + 2),
            (command.CommandId, 0x6C + 3)
        ]);
        memoryService.AllocateAndExecute(bytes);
        
        memoryService.WriteBytes(args, new byte[command.ParamData.Length]);
    }
}