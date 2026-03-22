// 

using System;
using SilkySouls.Interfaces;
using SilkySouls.memory;

namespace SilkySouls.Services
{
    public class ParamService(IMemoryService memoryService) : IParamService
    {
        public IntPtr GetParamRow(int tableIndex, int slotIndex, int rowIndex)
        {
            if (tableIndex >= 0x27 || slotIndex < 0) return IntPtr.Zero;
            var soloParamMan = memoryService.Read<nint>(Offsets.SoloParamMan.Base);

            var entry = soloParamMan + tableIndex * 0x48;
            if (slotIndex >= memoryService.Read<int>((IntPtr)entry + 0x10)) return IntPtr.Zero;

            var tableEntry = (IntPtr)memoryService.Read<nint>((IntPtr) entry + 0x18 + slotIndex * 8);
            if (tableEntry == IntPtr.Zero) return IntPtr.Zero;

            var header = (IntPtr)memoryService.Read<nint>(tableEntry + 0x38);
            var dataOffset = memoryService.Read<int>(header + 0x34 + rowIndex * 0x0C);
            return header + dataOffset;
        }

        public void WriteInt32(IntPtr row, int offset, int value)
        {
            memoryService.Write(row + offset, value);
        }

        public void WriteFloat(IntPtr row, int offset, float value)
        {
            memoryService.Write(row + offset, value);
        }

        public void WriteInt16(IntPtr row, int offset, short value)
        {
            memoryService.WriteBytes(row + offset, BitConverter.GetBytes(value));
        }

        public void WriteByte(IntPtr row, int offset, byte value)
        {
            memoryService.Write(row + offset, value);
        }
    }
}