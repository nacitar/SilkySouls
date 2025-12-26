// 

using System;

namespace SilkySouls
{
    public interface IParamService
    {
        IntPtr GetParamRow(int tableIndex, int slotIndex, int rowIndex);
        void WriteInt32(IntPtr row, int offset, int value);
        void WriteFloat(IntPtr row, int offset, float value);
        void WriteInt16(IntPtr row, int offset, short value);
        void WriteByte(IntPtr row, int offset, byte value);
    }
}