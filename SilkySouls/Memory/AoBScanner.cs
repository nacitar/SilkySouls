using System;
using System.Collections.Generic;
using System.IO;
using SilkySouls.Interfaces;
using SilkySouls.memory;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Memory
{
    public class AoBScanner
    {
        private readonly IMemoryService _memoryService;

        public AoBScanner(IMemoryService memoryService)
        {
            _memoryService = memoryService;
        }

        public void Scan()
        {
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkySouls");
            Directory.CreateDirectory(appData);
            string savePath = Path.Combine(appData, "backup_addresses.txt");
            
            Dictionary<string, long> saved = new Dictionary<string, long>();
            if (File.Exists(savePath))
            {
                foreach (string line in File.ReadAllLines(savePath))
                {
                    string[] parts = line.Split('=');
                    saved[parts[0]] = Convert.ToInt64(parts[1], 16);
                }
            }
            
            
            WorldChrMan.Base = FindAddressByPattern(Patterns.WorldChrMan);
            DebugFlags.Base = FindAddressByPattern(Patterns.DebugFlags);
            GameDataMan.Base = FindAddressByPattern(Patterns.GameDataMan);
            Functions.ItemGet = FindAddressByPattern(Patterns.ItemGetFunc);
            ItemGetMenuManImpl.Base = FindAddressByPattern(Patterns.ItemGetMenuMan);
            Functions.ItemDlgFunc = FindAddressByPattern(Patterns.ItemGetDlgFunc);
            FieldArea.Base = FindAddressByPattern(Patterns.FieldArea);
            GameMan.Base = FindAddressByPattern(Patterns.GameMan);
            DamageManager.Base = FindAddressByPattern(Patterns.DamMan);
            MenuMan.Base = FindAddressByPattern(Patterns.MenuMan);
            EventFlagMan.Base = FindAddressByPattern(Patterns.EventFlagMan);
            Functions.LevelUpFunc = FindAddressByPattern(Patterns.LevelUpFunc);
            Functions.RestoreCastsFunc = FindAddressByPattern(Patterns.RestoreCastsFunc);
            HgDraw.Base = FindAddressByPattern(Patterns.HgDraw);
            EventMan.Base = FindAddressByPattern(Patterns.WarpEvent);
            Functions.Warp = FindAddressByPattern(Patterns.WarpFunc);
            SoloParamMan.Base = FindAddressByPattern(Patterns.SoloParamMan);
     
            WorldAiMan.Base = FindAddressByPattern(Patterns.WorldAiMan);
            EmkEventIns.Base = FindAddressByPattern(Patterns.EmkEventIns);
            DebugEventMan.Base = FindAddressByPattern(Patterns.DebugEventMan);

            // Hooks
            TryPatternWithFallback("LastLockedTarget", Patterns.LastLockedTarget,
                addr => Hooks.LastLockedTarget = addr, saved);
            TryPatternWithFallback("AllNoDamage", Patterns.AllNoDamage,
                addr => Hooks.AllNoDamage = addr, saved);
            TryPatternWithFallback("Draw", Patterns.DrawHook, addr => Hooks.Draw = addr, saved);
            TryPatternWithFallback("InAirTimer", Patterns.InAirTimer, addr => Hooks.InAirTimer = addr,
                saved);
            TryPatternWithFallback("Keyboard", Patterns.Keyboard, addr => Hooks.Keyboard = addr,
                saved);
            TryPatternWithFallback("ControllerR2", Patterns.ControllerR2,
                addr => Hooks.ControllerR2 = addr, saved);
            TryPatternWithFallback("ControllerL2", Patterns.ControllerL2,
                addr => Hooks.ControllerL2 = addr, saved);
            TryPatternWithFallback("UpdateCoords", Patterns.UpdateCoords,
                addr => Hooks.UpdateCoords = addr, saved);
            TryPatternWithFallback("WarpCoords", Patterns.WarpCoords, addr => Hooks.WarpCoords = addr,
                saved);
            TryPatternWithFallback("LuaIfCase", Patterns.LuaLowerOrEqualHook,
                addr => Hooks.LuaLowerOrEqual = addr, saved);
            TryPatternWithFallback("LuaSwitchCase", Patterns.LuaOpCodeSwitch,
                addr => Hooks.LuaVmSwitch = addr, saved);
            TryPatternWithFallback("BattleActivate", Patterns.BattleActivateHook,
                addr => Hooks.BattleActivate = addr, saved); 
            TryPatternWithFallback("Emevd", Patterns.EmevdCommandHook,
                addr => Hooks.Emevd = addr, saved);

// Patches
            TryPatternWithFallback("FourKingsPatch", Patterns.FourKingsPatch,
                addr => Patches.FourKingsPatch = addr, saved);
            TryPatternWithFallback("NoRollPatch", Patterns.NoRollPatch, addr => Patches.NoRollPatch = addr,
                saved);
            TryPatternWithFallback("InfiniteDurabilityPatch", Patterns.InfiniteDurabilityPatch,
                addr => Patches.InfiniteDurabilityPatch = addr, saved);
            TryPatternWithFallback("DrawEventPatch", Patterns.DrawEventPatch,
                addr => Patches.DrawEventPatch = addr, saved);
            TryPatternWithFallback("DrawSoundViewPatch", Patterns.DrawSoundViewPatch,
                addr => Patches.DrawSoundViewPatch = addr, saved);
            TryPatternWithFallback("QuitoutPatch", Patterns.QuitoutPatch, addr => Patches.QuitoutPatch = addr,
                saved);
            
            using (var writer = new StreamWriter(savePath))
            {
                foreach (var pair in saved)
                    writer.WriteLine($"{pair.Key}={pair.Value:X}");
            }
            
            
            Functions.SetEvent = FindAddressByPattern(Patterns.SetEvent);
            Functions.GetEvent = FindAddressByPattern(Patterns.GetEvent);
            Functions.ShopParamSave = FindAddressByPattern(Patterns.ShopParamSave);
            Functions.OpenRegularShop = FindAddressByPattern(Patterns.OpenRegularShop);
            Functions.ExecuteEmevdCommand = FindAddressByPattern(Patterns.ProcessEmevdCommand);
            Functions.OpenAttunement = FindAddressByPattern(Patterns.OpenAttunement);
            Functions.AttunementWindowPrep = FindAddressByPattern(Patterns.AttunementWindowPrep);
            Functions.GetInventoryIndexByCatAndId = FindAddressByPattern(Patterns.GetInventoryIndexByCatAndId);
            
            
            #if DEBUG
            Console.WriteLine($"WorldChrMan.Base: 0x{WorldChrMan.Base:X}");
            Console.WriteLine($"DebugFlags.Base: 0x{DebugFlags.Base:X}");
            Console.WriteLine($"GameDataMan.Base: 0x{GameDataMan.Base:X}");
            Console.WriteLine($"ItemGet: 0x{Functions.ItemGet:X}");
            Console.WriteLine($"ItemGetMenuMan: 0x{ItemGetMenuManImpl.Base:X}");
            Console.WriteLine($"ItemDlgFunc: 0x{Functions.ItemDlgFunc:X}");
            Console.WriteLine($"FieldArea.Base: 0x{FieldArea.Base:X}");
            Console.WriteLine($"GameMan.Base: 0x{GameMan.Base:X}");
            Console.WriteLine($"DamageMan.Base: 0x{DamageManager.Base:X}");
            Console.WriteLine($"DrawEventPatch: 0x{Patches.DrawEventPatch:X}");
            Console.WriteLine($"DrawSoundViewPatch: 0x{Patches.DrawSoundViewPatch:X}");
            Console.WriteLine($"MenuMan.Base: 0x{MenuMan.Base:X}");
            Console.WriteLine($"EventFlagMan.Base: 0x{EventFlagMan.Base:X}");
            Console.WriteLine($"LevelUpFunc: 0x{Functions.LevelUpFunc:X}");
            Console.WriteLine($"RestoreCastsFunc: 0x{Functions.RestoreCastsFunc:X}");
            Console.WriteLine($"HgDraw.Base: 0x{HgDraw.Base:X}");
            Console.WriteLine($"EventMan: 0x{(long)EventMan.Base:X}");
            Console.WriteLine($"Warp: 0x{(long)Functions.Warp:X}");
            Console.WriteLine($"FastQuitout: 0x{Patches.QuitoutPatch:X}");
            Console.WriteLine($"WorldAiMan: 0x{WorldAiMan.Base:X}");
            Console.WriteLine($"EmkEventIns: 0x{EmkEventIns.Base:X}");
            Console.WriteLine($"DebugEventMan: 0x{DebugEventMan.Base:X}");
            Console.WriteLine($"SoloParamMan: 0x{SoloParamMan.Base:X}");

            Console.WriteLine($"Hooks.LastLockedTarget: 0x{Hooks.LastLockedTarget:X}");
            Console.WriteLine($"Hooks.AllNoDamage: 0x{Hooks.AllNoDamage:X}");
            Console.WriteLine($"Hooks.Draw: 0x{Hooks.Draw:X}");
            Console.WriteLine($"Hooks.InAirTimer: 0x{Hooks.InAirTimer:X}");
            Console.WriteLine($"Hooks.Keyboard: 0x{Hooks.Keyboard:X}");
            Console.WriteLine($"Hooks.ControllerR2: 0x{Hooks.ControllerR2:X}");
            Console.WriteLine($"Hooks.ControllerL2: 0x{Hooks.ControllerL2:X}");
            Console.WriteLine($"Hooks.UpdateCoords: 0x{Hooks.UpdateCoords:X}");
            Console.WriteLine($"Hooks.WarpCoords: 0x{Hooks.WarpCoords:X}");
            Console.WriteLine($"Hooks.LuaIfElse: 0x{(long)Hooks.LuaLowerOrEqual:X}");
            Console.WriteLine($"Hooks.Emevd: 0x{Hooks.Emevd:X}");
            Console.WriteLine($"Hooks.Draw: 0x{Hooks.Draw:X}");
            Console.WriteLine($"Patches.InfiniteDurabilityPatch: 0x{Patches.InfiniteDurabilityPatch:X}");
            
            Console.WriteLine($"Funcs.SetEvent: 0x{Functions.SetEvent:X}");
            Console.WriteLine($"Funcs.ShopParamSave: 0x{Functions.ShopParamSave:X}");
            Console.WriteLine($"Funcs.OpenRegularShop: 0x{Functions.OpenRegularShop:X}");
            Console.WriteLine($"Funcs.ProcessEmevdCommand: 0x{Functions.ExecuteEmevdCommand:X}");
            Console.WriteLine($"Funcs.OpenAttunement: 0x{Functions.OpenAttunement:X}");
            Console.WriteLine($"Funcs.AttunementWindowPrep: 0x{Functions.AttunementWindowPrep:X}");
            Console.WriteLine($"Funcs.GetEvent: 0x{Functions.GetEvent:X}");
#endif
        }
        
        private void TryPatternWithFallback(string name, Pattern pattern, Action<IntPtr> setter, Dictionary<string, long> saved)
        {
            var addr = FindAddressByPattern(pattern);
    
            if (addr == IntPtr.Zero && saved.TryGetValue(name, out var value))
                addr = new IntPtr(value);
            else if (addr != IntPtr.Zero)
                saved[name] = addr.ToInt64();

            setter(addr);
        }

        public IntPtr FindAddressByPattern(Pattern pattern)
        {
            var results = FindAddressesByPattern(pattern, 1);
            return results.Count > 0 ? results[0] : IntPtr.Zero;
        }

        public List<IntPtr> FindAddressesByPattern(Pattern pattern, int size)
        {
            List<IntPtr> addresses = PatternScanMultiple(pattern.Bytes, pattern.Mask, size);

            for (int i = 0; i < addresses.Count; i++)
            {
                IntPtr instructionAddress = IntPtr.Add(addresses[i], pattern.InstructionOffset);

                switch (pattern.AddressingMode)
                {
                    case AddressingMode.Absolute:
                        addresses[i] = instructionAddress;
                        break;
                    // case AddressingMode.Direct32:
                    // {
                    //     uint absoluteAddr = _memoryIo.ReadUInt32(IntPtr.Add(instructionAddress, pattern.OffsetLocation));
                    //     addresses[i] = (IntPtr)absoluteAddr;
                    //     break;
                    // }
                    default:
                    {
                        int offset = _memoryService.Read<int>(IntPtr.Add(instructionAddress, pattern.OffsetLocation));
                        addresses[i] = IntPtr.Add(instructionAddress, offset + pattern.InstructionLength);
                        break;
                    }
                }
            }

            return addresses;
        }
        
        private List<IntPtr> PatternScanMultiple(byte[] pattern, string mask, int size)
        {
            const int chunkSize = 4096 * 16;
            byte[] buffer = new byte[chunkSize];

            IntPtr currentAddress = _memoryService.BaseAddress;
            IntPtr endAddress = IntPtr.Add(currentAddress, 0x3200000);

            List<IntPtr> addresses = new List<IntPtr>();

            while (currentAddress.ToInt64() < endAddress.ToInt64())
            {
                int bytesRemaining = (int)(endAddress.ToInt64() - currentAddress.ToInt64());
                int bytesToRead = Math.Min(bytesRemaining, buffer.Length);

                if (bytesToRead < pattern.Length)
                    break;

                buffer = _memoryService.ReadBytes(currentAddress, bytesToRead);

                for (int i = 0; i <= bytesToRead - pattern.Length; i++)
                {
                    bool found = true;

                    for (int j = 0; j < pattern.Length; j++)
                    {
                        if (j < mask.Length && mask[j] == '?')
                            continue;

                        if (buffer[i + j] != pattern[j])
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                        addresses.Add(IntPtr.Add(currentAddress, i));
                    if (addresses.Count == size) break;
                }

                currentAddress = IntPtr.Add(currentAddress, bytesToRead - pattern.Length + 1);
            }

            return addresses;
        }

        public IntPtr PatternScan(byte[] pattern, string mask)
        {
            const int chunkSize = 4096 * 16;
            byte[] buffer = new byte[chunkSize];

            IntPtr currentAddress = _memoryService.BaseAddress;
            IntPtr endAddress = IntPtr.Add(currentAddress, 0x3200000);

            while (currentAddress.ToInt64() < endAddress.ToInt64())
            {
                int bytesRemaining = (int)(endAddress.ToInt64() - currentAddress.ToInt64());
                int bytesToRead = Math.Min(bytesRemaining, buffer.Length);

                if (bytesToRead < pattern.Length)
                    break;

                buffer = _memoryService.ReadBytes(currentAddress, bytesToRead);

                for (int i = 0; i <= bytesToRead - pattern.Length; i++)
                {
                    bool found = true;

                    for (int j = 0; j < pattern.Length; j++)
                    {
                        if (j < mask.Length && mask[j] == '?')
                            continue;

                        if (buffer[i + j] != pattern[j])
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                        return IntPtr.Add(currentAddress, i);
                }

                currentAddress = IntPtr.Add(currentAddress, bytesToRead - pattern.Length + 1);
            }

            return IntPtr.Zero;
        }
    }
}