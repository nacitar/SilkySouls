using System;
using System.Collections.Generic;
using System.Linq;
using SilkySouls.Enums;
using SilkySouls.Interfaces;

namespace SilkySouls.Memory
{
    public class HookManager
    {
        private readonly Dictionary<nint, HookData> _hookRegistry = new();
        private readonly IMemoryService _memoryService;

        public HookManager(IMemoryService memoryService, IStateService stateService)
        {
            _memoryService = memoryService;
            stateService.Subscribe(State.Detached, ClearHooks);
        }

        private class HookData
        {
            public nint OriginAddr { get; set; }
            public nint CaveAddr { get; set; }
            public byte[] OriginalBytes { get; set; }
        }

        public nint InstallHook(nint codeLoc, nint origin, byte[] originalBytes)
        {
            byte[] hookBytes = GetHookBytes(originalBytes.Length, codeLoc, origin);
            _memoryService.WriteBytes(origin, hookBytes);
            _hookRegistry[codeLoc] = new HookData
            {
                CaveAddr = codeLoc,
                OriginAddr = origin,
                OriginalBytes = originalBytes
            };
            return codeLoc;
        }

        private byte[] GetHookBytes(int originalBytesLength, nint target, nint origin)
        {
            byte[] hookBytes = new byte[originalBytesLength];
            hookBytes[0] = 0xE9;

            int jumpOffset = (int)(target - (origin + 5));
            byte[] offsetBytes = BitConverter.GetBytes(jumpOffset);
            Array.Copy(offsetBytes, 0, hookBytes, 1, 4);

            for (int i = 5; i < hookBytes.Length; i++)
            {
                hookBytes[i] = 0x90;
            }

            return hookBytes;
        }

        public void UninstallHook(nint key)
        {
            if (!_hookRegistry.TryGetValue(key, out HookData hookToUninstall)) return;

            _memoryService.WriteBytes(hookToUninstall.OriginAddr, hookToUninstall.OriginalBytes);
            _hookRegistry.Remove(key);
        }

        private void ClearHooks() => _hookRegistry.Clear();

        public void UninstallAllHooks()
        {
            foreach (var key in _hookRegistry.Keys.ToList())
            {
                UninstallHook(key);
            }
        }
    }
}