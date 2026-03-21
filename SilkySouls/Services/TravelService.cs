using System;
using System.Threading;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Models;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services
{
    public class TravelService(IMemoryService memoryService, HookManager hookManager)
    {
        public void Warp(WarpLocation selectedWarpLocation)
        {
            var lastBonfireAdr =
                memoryService.FollowPointers(memoryService.Read<nint>(GameMan.Base), new[] { GameMan.LastBonfire }, false);

            memoryService.Write(lastBonfireAdr, selectedWarpLocation.Id);

            byte[] warpBytes = AsmLoader.GetAsmBytes(AsmScript.Warp);
            byte[] bytes = BitConverter.GetBytes(WarpEvent.ToInt64());
            Array.Copy(bytes, 0, warpBytes, 2, 8);
            bytes = BitConverter.GetBytes(WarpFunc);
            Array.Copy(bytes, 0, warpBytes, 24, 8);

            memoryService.AllocateAndExecute(warpBytes);

            if (selectedWarpLocation.HasCoordinates)
            {
                var coordsAddr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.WarpCoords.Coords;
                var coordsOrigin = Hooks.WarpCoords;
                var coordCodeBlockAddr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.WarpCoords.CoordCode;

                byte[] coords = new byte[4 * sizeof(float)];
                Buffer.BlockCopy(selectedWarpLocation.Coords, 0, coords, 0,
                    Math.Min(selectedWarpLocation.Coords.Length, 3) * sizeof(float));
                BitConverter.GetBytes(1.0f).CopyTo(coords, 3 * sizeof(float));

                memoryService.WriteBytes(coordsAddr, coords);

                byte[] coordWarpBytes = AsmLoader.GetAsmBytes(AsmScript.WarpCoords);
                bytes = BitConverter.GetBytes(coordsAddr);
                Array.Copy(bytes, 0, coordWarpBytes, 3, 8);
                int originOffset = (int)(coordsOrigin + 8 - (coordCodeBlockAddr + 33));
                bytes = BitConverter.GetBytes(originOffset);
                Array.Copy(bytes, 0, coordWarpBytes, 29, 4);

                memoryService.WriteBytes(coordCodeBlockAddr, coordWarpBytes);

                var angleAddr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.WarpCoords.Angle;
                var angleOrigin = coordsOrigin + 0x40;
                var angleCodeBlockAddr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.WarpCoords.AngleCode;

                byte[] angle = new byte[16];
                bytes = BitConverter.GetBytes(selectedWarpLocation.Angle);
                Array.Copy(bytes, 0, angle, 4, 4);
                memoryService.WriteBytes(angleAddr, angle);

                byte[] angleWarpBytes = AsmLoader.GetAsmBytes(AsmScript.WarpAngle);
                bytes = BitConverter.GetBytes(angleAddr);
                Array.Copy(bytes, 0, angleWarpBytes, 3, 8);
                originOffset = (int)(angleOrigin + 8 - (angleCodeBlockAddr + 33));
                bytes = BitConverter.GetBytes(originOffset);
                Array.Copy(bytes, 0, angleWarpBytes, 29, 4);
                memoryService.WriteBytes(angleCodeBlockAddr, angleWarpBytes);

                IntPtr loadingFlagAddr =
                    memoryService.FollowPointers(memoryService.Read<nint>(MenuMan.Base), new[] { (int)MenuMan.MenuManData.LoadedFlag }, false);

                if (!WaitForLoadingFlag(loadingFlagAddr, 0))
                {
                    return;
                }

                hookManager.InstallHook(coordCodeBlockAddr, coordsOrigin,
                    new byte[] { 0x66, 0x0F, 0x7F, 0x80, 0x80, 0x0A, 0x00, 0x00 });
                hookManager.InstallHook(angleCodeBlockAddr, angleOrigin,
                    new byte[] { 0x66, 0x0F, 0x7F, 0x80, 0x90, 0x0A, 0x00, 0x00 });


                if (!WaitForLoadingFlag(loadingFlagAddr, 1))
                {
                }

                hookManager.UninstallHook(coordCodeBlockAddr);
                hookManager.UninstallHook(angleCodeBlockAddr);
            }
        }

        private bool WaitForLoadingFlag(IntPtr loadingFlagAddr, int expectedValue)
        {
            int startTime = Environment.TickCount;

            while (Environment.TickCount - startTime < 10000)
            {
                int loadingValue = memoryService.Read<int>(loadingFlagAddr);
                if (loadingValue == expectedValue)
                {
                    return true;
                }

                Thread.Sleep(50);
            }

            return false;
        }

        public void UnlockBonfireWarps()
        {
            var bonfireFlagBase = memoryService.FollowPointers(memoryService.Read<nint>(EventFlagMan.Base),
                new[] { EventFlagMan.FlagPtr }, true);

            var bonfireWarpFlagAddr = bonfireFlagBase + EventFlagMan.WarpFlag;
            memoryService.SetBit32(bonfireWarpFlagAddr, EventFlagMan.WarpFlagBit1, true);
            memoryService.SetBit32(bonfireWarpFlagAddr, EventFlagMan.WarpFlagBit2, true);

            var bonfireFlagAddr = bonfireFlagBase + EventFlagMan.BonfireFlags;
            foreach (EventFlagMan.BonfireBitFlag flag in Enum.GetValues(
                         typeof(EventFlagMan.BonfireBitFlag)))
            {
                int bitPosition = (int)flag;
                memoryService.SetBit32(bonfireFlagAddr, bitPosition, true);
            }
        }
    }
}