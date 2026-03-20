// 

using System;
using System.Diagnostics;

namespace SilkySouls.Interfaces;

public interface IMemoryService
{
    public bool IsAttached { get; }
    public Process? TargetProcess { get; }
    public nint BaseAddress { get; }
    public int ModuleMemorySize { get; }
    
    string ReadString(nint addr, int maxLength = 32);
    byte[] ReadBytes(nint addr, int size);
    public nint FollowPointers(nint baseAddress, int[] offsets, bool readFinalPtr, bool derefBase = true);

    T[] ReadArray<T>(IntPtr addr, int count) where T : unmanaged;
    T Read<T>(IntPtr addr) where T : unmanaged;
    string HexDump(nint addr, int size);

    void Write<T>(IntPtr addr, T value) where T : unmanaged;
    void Write(IntPtr addr, bool value);
    void WriteString(nint addr, string value, int maxLength = 32);
    void WriteBytes(IntPtr addr, byte[] val);
    void SetBit32(IntPtr addr, int bitPosition, bool setValue);
    void SetBitValue(nint addr, int flagMask, bool setValue);
    bool IsBitSet(nint addr, int flagMask);
    IntPtr GetProcAddress(string moduleName, string procName);
    void RunThread(nint address, uint timeout = uint.MaxValue);
    bool RunThreadAndWaitForCompletion(IntPtr address, uint timeout = 0xFFFFFFFF);
    void RunPersistentThread(IntPtr address);

    void AllocateAndExecute(byte[] shellcode);
    void AllocCodeCave();

    nint GetModuleStart(nint address);

    nint AllocateMem(uint size);
    void FreeMem(nint addr);
    
    void StartAutoAttach();
}