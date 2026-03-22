using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services
{
    public class EventService(IMemoryService memoryService)
    {
        
        public void ToggleDisableEvents(bool isDisableEventsEnabled)
        {
            memoryService.Write(memoryService.Read<nint>(DebugEventMan.Base) + DebugEventMan.DisableEvents,
                isDisableEventsEnabled ? (byte)1 : (byte)0);
        }

        public void SetEvent(int eventId, bool setVal)
        {
            var bytes = AsmLoader.GetAsmBytes(AsmScript.SetEvent);
            AsmHelper.WriteAbsoluteAddresses(bytes, [
                (memoryService.Read<nint>(EventFlagMan.Base), 2 ),
                (eventId, 0xA + 2),
                (setVal ? 1 : 0, 0x14 + 2),
                (Functions.SetEvent, 0x28 + 2)
            ]);
            memoryService.AllocateAndExecute(bytes);
        }

        public void SetMultipleEventsOn(params int[] flagIds)
        {
            foreach (var flagId in flagIds)
            {
                SetEvent(flagId, true);
            }
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
        
        public void RingGargBell()
        {
            SetEvent(GameIdsOld.EventFlags.GargBell, true);
            if (GetEvent(GameIdsOld.EventFlags.QuelaagBell))SetEvent(GameIdsOld.EventFlags.Sens, true);
        }
        public void RingQuelaagBell()
        {
            SetEvent(GameIdsOld.EventFlags.QuelaagBell, true);
            if (GetEvent(GameIdsOld.EventFlags.GargBell))SetEvent(GameIdsOld.EventFlags.Sens, true);
        }
        
        public void OpenSensGate(int sens)
        {
            SetEvent(sens, true);
            // ExecuteEmevdCommand(GameIdsOld.EmevdCommands.ReproduceObjectAnimation,
            //     GameIdsOld.EmevdCommandParams.SensDoor);
        }

        
        public void PlaceLordVessel()
        {
            SetEvent(GameIdsOld.EventFlags.PlaceLordVessel, true);
            SetEvent(GameIdsOld.EventFlags.DukesAfterLordVessel, true);
            // ExecuteEmevdCommand(GameIdsOld.EmevdCommands.DeactiveObject, GameIdsOld.EmevdCommandParams.DukesFogDeactiveObject);
            // await Task.Delay(5);
            // ExecuteEmevdCommand(GameIdsOld.EmevdCommands.DeleteMapSfx, GameIdsOld.EmevdCommandParams.DukesFogDeleteMapSfx);
            // await Task.Delay(5);
            // ExecuteEmevdCommand(GameIdsOld.EmevdCommands.DeactiveObject, GameIdsOld.EmevdCommandParams.DemonRuinsFogDeactiveObject);
            // await Task.Delay(5);
            // ExecuteEmevdCommand(GameIdsOld.EmevdCommands.DeleteMapSfx, GameIdsOld.EmevdCommandParams.DemonRuinsFogDeleteMapSfx);
            // await Task.Delay(5);
            // ExecuteEmevdCommand(GameIdsOld.EmevdCommands.DeactiveObject, GameIdsOld.EmevdCommandParams.NitoFogDeactiveObject);
            // await Task.Delay(5);
            // ExecuteEmevdCommand(GameIdsOld.EmevdCommands.DeleteMapSfx, GameIdsOld.EmevdCommandParams.NitoFogDeleteMapSfx);
            // await Task.Delay(500);
        }
    }
}