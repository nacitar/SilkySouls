using System;
using System.Threading;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services
{
    public class PlayerServiceOld(IMemoryService memoryService)
    {
        
        
        public void SavePos(int index)
        {
            var coordsPtr = GetPlayerCoordinatePtr(WorldChrMan.Coords.X);

            byte[] positionBytes = memoryService.ReadBytes(coordsPtr, 12);

            if (index == 0)
            {
                memoryService.WriteBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos1, positionBytes);
            }
            else
            {
                memoryService.WriteBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos2, positionBytes);
            }
        }

        public IntPtr GetPlayerCoordinatePtr(WorldChrMan.Coords coordinateType)
        {
            return memoryService.FollowPointers(memoryService.Read<nint>(WorldChrMan.Base), new[]
            {
                (int)WorldChrMan.PlayerIns,
                (int)WorldChrMan.PlayerInsOffsets.CoordsPtr1,
                WorldChrMan.CoordsPtr2,
                WorldChrMan.CoordsPtr3,
                WorldChrMan.CoordsPtr4,
                (int)coordinateType
            }, false);
        }

        private byte[] _lastKnownCoordsBytes;

        public void RestorePos(int index)
        {
            var coordsUpdate = (IntPtr)Hooks.UpdateCoords;
            byte[] originBytes = memoryService.ReadBytes(coordsUpdate, 7);
            bool allNops = true;
            for (int i = 0; i < originBytes.Length; i++)
            {
                if (originBytes[i] != 0x90)
                {
                    allNops = false;
                    break;
                }
            }

            if (allNops)
            {
                originBytes = _lastKnownCoordsBytes;
            }

            _lastKnownCoordsBytes = originBytes;
            memoryService.WriteBytes(coordsUpdate, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            memoryService.WriteBytes(coordsUpdate + 0x252, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });

            byte[] positionBytes;
            if (index == 0)
            {
                positionBytes = memoryService.ReadBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos1, 12);
            }
            else
            {
                positionBytes = memoryService.ReadBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos2, 12);
            }

            var coordsPtr = GetPlayerCoordinatePtr(WorldChrMan.Coords.X);

            memoryService.WriteBytes(coordsPtr, positionBytes);
            Thread.Sleep(15);
            memoryService.WriteBytes(coordsUpdate, originBytes);
            memoryService.WriteBytes(coordsUpdate + 0x252, new byte[] { 0x0F, 0x29, 0x81, 0x20, 0x01, 0x00, 0x00 });
        }
    }
}