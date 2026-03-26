using System;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services
{
    public class UtilityServiceOld(IMemoryService memoryService, HookManager hookManager)
    {

        
        public void ToggleFilter(bool value)
        {
            if (value)
            {
                var filterPtr = memoryService.FollowPointers(memoryService.Read<nint>(FieldArea.Base), new[]
                    { FieldArea.RenderPtr, FieldArea.FilterRemoval }, false);
                memoryService.Write(filterPtr, (byte)1);
                var brightnessPtr = memoryService.FollowPointers(memoryService.Read<nint>(FieldArea.Base), new[]
                    { FieldArea.RenderPtr, FieldArea.Brightness }, false);
                var bytes = new byte[12];
                var floatBytes = BitConverter.GetBytes(5.0f);
                Buffer.BlockCopy(floatBytes, 0, bytes, 0, 4);
                Buffer.BlockCopy(floatBytes, 0, bytes, 4, 4);
                Buffer.BlockCopy(floatBytes, 0, bytes, 8, 4);

                memoryService.WriteBytes(brightnessPtr, bytes);
            }
            else
            {
                var filterPtr = memoryService.FollowPointers(memoryService.Read<nint>(FieldArea.Base), new[]
                    { FieldArea.RenderPtr, FieldArea.FilterRemoval }, false);
                memoryService.Write(filterPtr, (byte)0);
                var brightnessPtr = memoryService.FollowPointers(memoryService.Read<nint>(FieldArea.Base), new[]
                    { FieldArea.RenderPtr, FieldArea.Brightness }, false);
                var bytes = new byte[12];
                var floatBytes = BitConverter.GetBytes(1.0f);
                Buffer.BlockCopy(floatBytes, 0, bytes, 0, 4);
                Buffer.BlockCopy(floatBytes, 0, bytes, 4, 4);
                Buffer.BlockCopy(floatBytes, 0, bytes, 8, 4);

                memoryService.WriteBytes(brightnessPtr, bytes);
            }
        }
        
        
        public void SetGuaranteedBkhDrop(bool setValue)
        {
            var bkhPtr = memoryService.FollowPointers(memoryService.Read<nint>(SoloParamMan.Base), new[]
            {
                SoloParamMan.ParamResCap,
                SoloParamMan.ItemLot,
                SoloParamMan.BkhDropRateBase
            }, false);

            if (setValue)
            {
                memoryService.Write(bkhPtr + (int)SoloParamMan.BkhDropRateSlots.Nothing, (byte)0);
                memoryService.Write(bkhPtr + (int)SoloParamMan.BkhDropRateSlots.Bkh, (byte)0x64);
                memoryService.Write(bkhPtr + (int)SoloParamMan.BkhDropRateSlots.Bks, (byte)0);
            }
            else
            {
                memoryService.Write(bkhPtr + (int)SoloParamMan.BkhDropRateSlots.Nothing, (byte)0x4B);
                memoryService.Write(bkhPtr + (int)SoloParamMan.BkhDropRateSlots.Bkh, (byte)0x14);
                memoryService.Write(bkhPtr + (int)SoloParamMan.BkhDropRateSlots.Bks, (byte)0x5);
            }
        }
    }
}