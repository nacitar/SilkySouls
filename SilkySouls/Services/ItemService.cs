using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services
{
    public class ItemService(IMemoryService memoryService)
    {
        private bool _codeIsWritten;

        public void ItemSpawn(int itemId, int category, int quantity)
        {
            var shouldProcessFlag = CodeCaveOffsets.Base + (int)CodeCaveOffsets.ItemSpawn.ShouldProcessFlag;
            var code = CodeCaveOffsets.Base + (int)CodeCaveOffsets.ItemSpawn.Code;
            if (!_codeIsWritten)
            {
                
                var shouldExitFlag = CodeCaveOffsets.Base + (int)CodeCaveOffsets.ItemSpawn.ShouldExitFlag;
                
                var sleepAddr = memoryService.GetProcAddress("kernel32.dll", "Sleep");
                
                byte[] spawnBytes = AsmLoader.GetAsmBytes(AsmScript.ItemSpawn);
                AsmHelper.WriteRelativeOffsets(spawnBytes, new []
                {
                    (code, shouldProcessFlag, 7, 0x0 + 2),
                    (code + 0xD, shouldProcessFlag, 7, 0xD + 2),
                    (code + 0xA9, shouldExitFlag, 7, 0xA9 + 2)
                });
                
                AsmHelper.WriteAbsoluteAddresses(spawnBytes, [
                    (GameDataMan.Base, 0x2B + 2),
                    (Functions.ItemGet, 0x54 + 2),
                    (ItemGetMenuManImpl.Base, 0x64 + 2),
                    (Functions.ItemDlgFunc, 0x86 + 2),
                    (sleepAddr, 0x96 + 2)
                ]);
                
                memoryService.WriteBytes(code, spawnBytes);
                _codeIsWritten = true;
                memoryService.RunPersistentThread(code);
            }

            memoryService.Write(code + 0x14 + 1, category);
            memoryService.Write(code + 0x19 + 2, quantity);
            memoryService.Write(code + 0x1F + 2, itemId);

            memoryService.Write(code + 0x71 + 1, category);
            memoryService.Write(code + 0x76 + 2, quantity);
            memoryService.Write(code + 0x7C + 2, itemId);
            
            memoryService.Write(shouldProcessFlag, (byte)1);
            
        }
        
        
        public void Reset()
        {
            _codeIsWritten = false;
        }

        public void SignalClose()
        {
            memoryService.Write(CodeCaveOffsets.Base + (int)CodeCaveOffsets.ItemSpawn.ShouldExitFlag, (byte)1);
        }
    }
}