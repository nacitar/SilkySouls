// 

using System;
using System.IO;
using SilkySouls.Interfaces;
using SilkySouls.memory;

namespace SilkySouls.Utilities;

public static class PatchManager
{
    public static bool Initialize(IMemoryService memoryService)
    {
        if (memoryService.TargetProcess == null) return false;
        var module = memoryService.TargetProcess.MainModule;
        var fileInfo = new FileInfo(module.FileName);
        var fileSize = fileInfo.Length;
        var moduleBase = memoryService.BaseAddress;
        
        return Offsets.Initialize(fileSize, moduleBase);
    }
}