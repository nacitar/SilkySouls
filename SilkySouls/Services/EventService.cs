using System;
using System.Threading.Tasks;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services
{
    public class EventService(IMemoryService memoryService, HookManager hookManager)
    {
        private IntPtr _emevdCodeLoc;
        private bool _isEmevdCodeWritten;

        public void ToggleDisableEvents(bool isDisableEventsEnabled)
        {
            memoryService.Write(memoryService.Read<nint>(DebugEventMan.Base) + DebugEventMan.DisableEvents,
                isDisableEventsEnabled ? (byte)1 : (byte)0);
        }

        public void SetEvent(int flagId, int setVal)
        {
            var eventMan = memoryService.Read<nint>(EventFlagMan.Base);
            var setEventBytes = AsmLoader.GetAsmBytes(AsmScript.SetEvent);
            var bytes = BitConverter.GetBytes(eventMan);
            Array.Copy(bytes, 0, setEventBytes, 0x2, 8);
            bytes = BitConverter.GetBytes(flagId);
            Array.Copy(bytes, 0, setEventBytes, 0xA + 2, 8);
            bytes = BitConverter.GetBytes(setVal);
            Array.Copy(bytes, 0, setEventBytes, 0x14 + 2, 4);
            bytes = BitConverter.GetBytes(Funcs.SetEvent);
            Array.Copy(bytes, 0, setEventBytes, 0x24 + 2, 8);
            memoryService.AllocateAndExecute(setEventBytes);
        }

        public void SetMultipleEventsOn(params int[] flagIds)
        {
            foreach (var flagId in flagIds)
            {
                SetEvent(flagId, 1);
            }
        }

        public bool GetEvent(int eventId)
        {
            var getEventBytes = AsmLoader.GetAsmBytes(AsmScript.GetEvent);
            AsmHelper.WriteAbsoluteAddresses(getEventBytes, [
                (memoryService.Read<nint>(EventFlagMan.Base), 0x0 + 2),
                (eventId, 0xA + 2),
                (Funcs.GetEvent, 0x14 + 2),
                (CodeCaveOffsets.Base + CodeCaveOffsets.GetEventResult, 0x28 + 2)
            ]);
            
            memoryService.AllocateAndExecute(getEventBytes);
            return memoryService.Read<byte>(CodeCaveOffsets.Base + CodeCaveOffsets.GetEventResult) == 1;
        }
        
        public void RingGargBell()
        {
            SetEvent(GameIds.EventFlags.GargBell, 1);
            if (GetEvent(GameIds.EventFlags.QuelaagBell))SetEvent(GameIds.EventFlags.Sens, 1);
        }
        public void RingQuelaagBell()
        {
            SetEvent(GameIds.EventFlags.QuelaagBell, 1);
            if (GetEvent(GameIds.EventFlags.GargBell))SetEvent(GameIds.EventFlags.Sens, 1);
        }
        
        public async Task OpenSensGate(int sens)
        {
            SetEvent(sens, 1);
            ExecuteEmevdCommand(GameIds.EmevdCommands.ReproduceObjectAnimation,
                GameIds.EmevdCommandParams.SensDoor);
            await Task.Delay(1000);
            hookManager.UninstallHook(_emevdCodeLoc);
        }

        private void ExecuteEmevdCommand(int[] commandParams, int[] funcParams)
        {
            var hookLoc = Hooks.Emevd;
            var codeCaveBase = CodeCaveOffsets.Base;
            var commandParamsLoc = codeCaveBase + (int)CodeCaveOffsets.EmevdCommand.CommandParams;
            var funcParamsLoc = codeCaveBase + (int)CodeCaveOffsets.EmevdCommand.FuncParams;
            var flag = codeCaveBase + (int)CodeCaveOffsets.EmevdCommand.Flag;
            
            if (_isEmevdCodeWritten)
            {
                memoryService.Write(commandParamsLoc, commandParams[0]);
                memoryService.Write(commandParamsLoc + 0x4, commandParams[1]);
                memoryService.Write(funcParamsLoc, funcParams[0]);
                memoryService.Write(funcParamsLoc + 0x4, funcParams[1]);
                memoryService.Write(flag, (byte)0);
            }
            else
            {
                var xmmStorage = codeCaveBase + (int)CodeCaveOffsets.EmevdCommand.XmmStorage;
                var paramStruct = codeCaveBase + (int)CodeCaveOffsets.EmevdCommand.ParamStruct;
                _emevdCodeLoc = codeCaveBase + (int)CodeCaveOffsets.EmevdCommand.Code;

                memoryService.Write(commandParamsLoc, commandParams[0]);
                memoryService.Write(commandParamsLoc + 0x4, commandParams[1]);
                memoryService.Write(funcParamsLoc, funcParams[0]);
                memoryService.Write(funcParamsLoc + 0x4, funcParams[1]);

                // var codeBytes = AsmLoader.GetAsmBytes("ScriptCommands");
                // AsmHelper.WriteRelativeOffsets(codeBytes, new[]
                // {
                //     (_emevdCodeLoc.ToInt64(), flag.ToInt64(), 7, 0x2),
                //     (_emevdCodeLoc.ToInt64() + 0xD, flag.ToInt64(), 7, 0xD + 0x2),
                //     (_emevdCodeLoc.ToInt64() + 0x32, xmmStorage.ToInt64(), 8, 0x32 + 0x4),
                //     (_emevdCodeLoc.ToInt64() + 0x3A, xmmStorage.ToInt64() + 0x10, 8, 0x3A + 0x4),
                //     (_emevdCodeLoc.ToInt64() + 0x42, xmmStorage.ToInt64() + 0x20, 8, 0x42 + 0x4),
                //     (_emevdCodeLoc.ToInt64() + 0x4A, xmmStorage.ToInt64() + 0x30, 8, 0x4A + 0x4),
                //     (_emevdCodeLoc.ToInt64() + 0x52, xmmStorage.ToInt64() + 0x40, 8, 0x52 + 0x4),
                //     (_emevdCodeLoc.ToInt64() + 0x5A, xmmStorage.ToInt64() + 0x50, 8, 0x5A + 0x4),
                //     (_emevdCodeLoc.ToInt64() + 0x62, xmmStorage.ToInt64() + 0x60, 8, 0x62 + 0x4),
                //     (_emevdCodeLoc.ToInt64() + 0x6A, xmmStorage.ToInt64() + 0x70, 9, 0x6A + 0x5),
                //     (_emevdCodeLoc.ToInt64() + 0x73, xmmStorage.ToInt64() + 0x80, 9, 0x73 + 0x5),
                //     (_emevdCodeLoc.ToInt64() + 0x7C, paramStruct.ToInt64(), 7, 0x7C + 0x3),
                //     (_emevdCodeLoc.ToInt64() + 0x83, commandParamsLoc.ToInt64(), 7, 0x83 + 0x3),
                //     (_emevdCodeLoc.ToInt64() + 0x91, funcParamsLoc.ToInt64(), 7, 0x91 + 0x3),
                //     (_emevdCodeLoc.ToInt64() + 0x9F, EmkEventIns.Base.ToInt64(), 7, 0x9F + 0x3),
                //     (_emevdCodeLoc.ToInt64() + 0xB5, Funcs.ProcessEmevdCommand, 5, 0xB5 + 0x1),
                //     (_emevdCodeLoc.ToInt64() + 0xBE, xmmStorage.ToInt64() + 0x80, 9, 0xBE + 0x5),
                //     (_emevdCodeLoc.ToInt64() + 0xC7, xmmStorage.ToInt64() + 0x70, 9, 0xC7 + 0x5),
                //     (_emevdCodeLoc.ToInt64() + 0xD0, xmmStorage.ToInt64() + 0x60, 8, 0xD0 + 0x4),
                //     (_emevdCodeLoc.ToInt64() + 0xD8, xmmStorage.ToInt64() + 0x50, 8, 0xD8 + 0x4),
                //     (_emevdCodeLoc.ToInt64() + 0xE0, xmmStorage.ToInt64() + 0x40, 8, 0xE0 + 0x4),
                //     (_emevdCodeLoc.ToInt64() + 0xE8, xmmStorage.ToInt64() + 0x30, 8, 0xE8 + 0x4),
                //     (_emevdCodeLoc.ToInt64() + 0xF0, xmmStorage.ToInt64() + 0x20, 8, 0xF0 + 0x4),
                //     (_emevdCodeLoc.ToInt64() + 0xF8, xmmStorage.ToInt64() + 0x10, 8, 0xF8 + 0x4),
                //     (_emevdCodeLoc.ToInt64() + 0x100, xmmStorage.ToInt64(), 8, 0x100 + 0x4)
                // });

                // var jumpBytes = AsmHelper.GetJmpOriginOffsetBytes(hookLoc, 5, _emevdCodeLoc + 0x12D);
                // Array.Copy(jumpBytes, 0, codeBytes, 0x128 + 1, 4);
                //
                // memoryService.WriteBytes(_emevdCodeLoc, codeBytes);
                //
                // _isEmevdCodeWritten = true;
            }

            hookManager.InstallHook(_emevdCodeLoc, hookLoc, new byte[]
            {
                0xBA, 0x01, 0x00, 0x00, 0x00,
            });
        }

        public async Task PlaceLordVessel()
        {
            SetEvent(GameIds.EventFlags.PlaceLordVessel, 1);
            SetEvent(GameIds.EventFlags.DukesAfterLordVessel, 1);
            ExecuteEmevdCommand(GameIds.EmevdCommands.DeactiveObject, GameIds.EmevdCommandParams.DukesFogDeactiveObject);
            await Task.Delay(5);
            ExecuteEmevdCommand(GameIds.EmevdCommands.DeleteMapSfx, GameIds.EmevdCommandParams.DukesFogDeleteMapSfx);
            await Task.Delay(5);
            ExecuteEmevdCommand(GameIds.EmevdCommands.DeactiveObject, GameIds.EmevdCommandParams.DemonRuinsFogDeactiveObject);
            await Task.Delay(5);
            ExecuteEmevdCommand(GameIds.EmevdCommands.DeleteMapSfx, GameIds.EmevdCommandParams.DemonRuinsFogDeleteMapSfx);
            await Task.Delay(5);
            ExecuteEmevdCommand(GameIds.EmevdCommands.DeactiveObject, GameIds.EmevdCommandParams.NitoFogDeactiveObject);
            await Task.Delay(5);
            ExecuteEmevdCommand(GameIds.EmevdCommands.DeleteMapSfx, GameIds.EmevdCommandParams.NitoFogDeleteMapSfx);
            await Task.Delay(500);
            hookManager.UninstallHook(_emevdCodeLoc);
        }
    }
}