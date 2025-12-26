// 

using System;
using SilkySouls.memory;
using SilkySouls.Memory;

namespace SilkySouls.Services
{
    public class ParamService : IParamService
    {
        
        private readonly MemoryIo _memoryIo;
        
        public ParamService(MemoryIo memoryIo)
        {
            _memoryIo = memoryIo;
        }

        public IntPtr GetParamRow(int tableIndex, int slotIndex, int rowIndex)
        {
            if (tableIndex >= 0x27 || slotIndex < 0) return IntPtr.Zero;
            var soloParamMan = _memoryIo.ReadInt64(Offsets.SoloParamMan.Base);

            var entry = soloParamMan + tableIndex * 0x48;
            if (slotIndex >= _memoryIo.ReadInt32((IntPtr)entry + 0x10)) return IntPtr.Zero;

            var tableEntry = (IntPtr)_memoryIo.ReadUInt64((IntPtr) entry + 0x18 + slotIndex * 8);
            if (tableEntry == IntPtr.Zero) return IntPtr.Zero;

            var header = (IntPtr)_memoryIo.ReadUInt64(tableEntry + 0x38);
            var dataOffset = _memoryIo.ReadInt32(header + 0x34 + rowIndex * 0x0C);
            return header + dataOffset;
        }

        public void WriteInt32(IntPtr row, int offset, int value)
        {
            _memoryIo.WriteInt32(row + offset, value);
        }

        public void WriteFloat(IntPtr row, int offset, float value)
        {
            _memoryIo.WriteFloat(row + offset, value);
        }

        public void WriteInt16(IntPtr row, int offset, short value)
        {
            _memoryIo.WriteBytes(row + offset, BitConverter.GetBytes(value));
        }

        public void WriteByte(IntPtr row, int offset, byte value)
        {
            _memoryIo.WriteUInt8(row + offset, value);
        }
    }
}