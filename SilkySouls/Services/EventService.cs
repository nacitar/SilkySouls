// 

using SilkySouls.Enums;
using SilkySouls.GameIds;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.GameIds.Emevd;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services;

public class EventService(IMemoryService memoryService, IPlayerService playerService, IEmevdService emevdService)
    : IEventService
{
    public const int AnorLondoBlockId = 0xF010000;
    public const int TotgBlockId = 0xD010000;
    public const int DemonRuinsBlockId = 0xE010000;

    public void SetEvent(int eventId, bool setVal)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.SetEvent);
        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (memoryService.Read<nint>(EventFlagMan.Base), 2),
            (eventId, 0xA + 2),
            (setVal ? 1 : 0, 0x14 + 2),
            (Functions.SetEvent, 0x28 + 2)
        ]);
        memoryService.AllocateAndExecute(bytes);
    }

    public bool GetEvent(int eventId)
    {
        var getEventBytes = AsmLoader.GetAsmBytes(AsmScript.GetEvent);
        AsmHelper.WriteAbsoluteAddresses(getEventBytes, [
            (memoryService.Read<nint>(EventFlagMan.Base), 0x0 + 2),
            (eventId, 0xA + 2),
            (Functions.GetEvent, 0x14 + 2),
            (CodeCaveOffsets.Base + CodeCaveOffsets.GetEventResult, 0x28 + 2)
        ]);

        memoryService.AllocateAndExecute(getEventBytes);
        return memoryService.Read<byte>(CodeCaveOffsets.Base + CodeCaveOffsets.GetEventResult) == 1;
    }

    public void ToggleDisableEvents(bool isEnabled) =>
        memoryService.Write(memoryService.Read<nint>(DebugEventMan.Base) + DebugEventMan.DisableEvents, isEnabled);

    public void OpenSensGate()
    {
        SetEvent(EventFlags.Sens, true);
        emevdService.ExecuteEmevdCommand(
            EmevdCommands.ReproduceObjectAnimation(EntityId.SensDoor, 0));
    }

    public void PlaceLordVessel()
    {
        SetEvent(EventFlags.PlaceLordVessel, true);
        var currentBlockId =
            memoryService.FollowPointers(playerService.GetPlayerIns(), WorldChrMan.CurrentBlockId, false);

        switch (currentBlockId)
        {
            case AnorLondoBlockId:
                SetEvent(EventFlags.DukesAfterLordVessel, true);
                emevdService.ExecuteEmevdCommand(
                    EmevdCommands.SetObjectIsEnabled(EntityId.AnorLondoFogGateObject, false));
                emevdService.ExecuteEmevdCommand(
                    EmevdCommands.DeleteMapSfx(EntityId.AnorLondoFogGateSfx, false));
                return;
            case TotgBlockId:
                emevdService.ExecuteEmevdCommand(
                    EmevdCommands.SetObjectIsEnabled(EntityId.TotgFogGateObject, false));
                emevdService.ExecuteEmevdCommand(
                    EmevdCommands.DeleteMapSfx(EntityId.TotgFogGateSfx, false));
                return;
            case DemonRuinsBlockId:
                emevdService.ExecuteEmevdCommand(
                    EmevdCommands.SetObjectIsEnabled(EntityId.DemonRuinsFogGateObject, false));
                emevdService.ExecuteEmevdCommand(
                    EmevdCommands.DeleteMapSfx(EntityId.DemonRuinsFogGateSfx, false));
                return;
        }
    }
}