using System;
using System.Collections.Generic;
using System.Threading;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services
{
    public class PlayerServiceOld(IMemoryService memoryService)
    {
        
        
        public void SavePos(int index)
        {
            var coordsPtr = GetPlayerCoordinatePtr(WorldChrMan.Coords.X);

            byte[] positionBytes = memoryService.ReadBytes(coordsPtr, 12);

            if (index == 0)
            {
                memoryService.WriteBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos1, positionBytes);
            }
            else
            {
                memoryService.WriteBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos2, positionBytes);
            }
        }

        public IntPtr GetPlayerCoordinatePtr(WorldChrMan.Coords coordinateType)
        {
            return memoryService.FollowPointers(memoryService.Read<nint>(WorldChrMan.Base), new[]
            {
                (int)WorldChrMan.PlayerIns,
                (int)WorldChrMan.PlayerInsOffsets.CoordsPtr1,
                WorldChrMan.CoordsPtr2,
                WorldChrMan.CoordsPtr3,
                WorldChrMan.CoordsPtr4,
                (int)coordinateType
            }, false);
        }

        private byte[] _lastKnownCoordsBytes;

        public void RestorePos(int index)
        {
            var coordsUpdate = (IntPtr)Hooks.UpdateCoords;
            byte[] originBytes = memoryService.ReadBytes(coordsUpdate, 7);
            bool allNops = true;
            for (int i = 0; i < originBytes.Length; i++)
            {
                if (originBytes[i] != 0x90)
                {
                    allNops = false;
                    break;
                }
            }

            if (allNops)
            {
                originBytes = _lastKnownCoordsBytes;
            }

            _lastKnownCoordsBytes = originBytes;
            memoryService.WriteBytes(coordsUpdate, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            memoryService.WriteBytes(coordsUpdate + 0x252, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });

            byte[] positionBytes;
            if (index == 0)
            {
                positionBytes = memoryService.ReadBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos1, 12);
            }
            else
            {
                positionBytes = memoryService.ReadBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos2, 12);
            }

            var coordsPtr = GetPlayerCoordinatePtr(WorldChrMan.Coords.X);

            memoryService.WriteBytes(coordsPtr, positionBytes);
            Thread.Sleep(15);
            memoryService.WriteBytes(coordsUpdate, originBytes);
            memoryService.WriteBytes(coordsUpdate + 0x252, new byte[] { 0x0F, 0x29, 0x81, 0x20, 0x01, 0x00, 0x00 });
        }
        

        public void ToggleNoDeath(int value)
        {
            var noDeathPtr = DebugFlags.Base + DebugFlags.NoDeath;
            memoryService.Write(noDeathPtr, (byte)value);
        }


        public void ToggleNoDamage(bool setValue)
        {
            var noDamagePtr = memoryService.FollowPointers(memoryService.Read<nint>(WorldChrMan.Base),
                new[]
                {
                    (int)WorldChrMan.PlayerIns, (int)WorldChrMan.PlayerInsOffsets.NoDamage
                }, false);
            var flagMask = WorldChrMan.NoDamage;
            memoryService.SetBitValue(noDamagePtr, flagMask, setValue);
        }

        public void ToggleInfiniteStamina(bool setValue)
        {
            var infiniteStamPtr = memoryService.FollowPointers(memoryService.Read<nint>(WorldChrMan.Base),
                new[]
                {
                    (int)WorldChrMan.PlayerIns,
                    (int)WorldChrMan.PlayerInsOffsets.ChrFlags
                }, false);

            var flagMask = (byte)WorldChrMan.ChrFlags.InfiniteStam;
            memoryService.SetBitValue(infiniteStamPtr, flagMask, setValue);
        }

        public void ToggleNoGoodsConsume(bool setValue)
        {
            var noGoodsConsumePtr = memoryService.FollowPointers(memoryService.Read<nint>(WorldChrMan.Base),
                new[]
                {
                    (int)WorldChrMan.PlayerIns,
                    (int)WorldChrMan.PlayerInsOffsets.NoGoodsConsume
                }, false);
            var flagMask = WorldChrMan.NoGoodsConsume;
            memoryService.SetBitValue(noGoodsConsumePtr, flagMask, setValue);
        }

        public void ToggleInfiniteCasts(int value)
        {
            var infiniteCastsPtr = DebugFlags.Base + DebugFlags.InfiniteCasts;
            memoryService.Write(infiniteCastsPtr, (byte)value);
        }

        public void ToggleOneShot(int value)
        {
            var oneShotPtr = DebugFlags.Base + DebugFlags.OneShot;
            memoryService.Write(oneShotPtr, (byte)value);
        }

        public void ToggleInvisible(int value)
        {
            var invisiblePtr = DebugFlags.Base + DebugFlags.Invisible;
            memoryService.Write(invisiblePtr, (byte)value);
        }

        public void ToggleSilent(int value)
        {
            var silentPtr = DebugFlags.Base + DebugFlags.Silent;
            memoryService.Write(silentPtr, (byte)value);
        }

        public void ToggleNoAmmoConsume(int value)
        {
            var noAmmoConsumePtr = DebugFlags.Base + DebugFlags.NoAmmoConsume;
            memoryService.Write(noAmmoConsumePtr, (byte)value);
        }

        public void ToggleInfinitePoise(bool setValue)
        {
            var infinitePoisePtr = memoryService.FollowPointers(memoryService.Read<nint>(WorldChrMan.Base),
                new[]
                {
                    (int)WorldChrMan.PlayerIns,
                    (int)WorldChrMan.PlayerInsOffsets.InfinitePoise
                }, false);
            var flagMask = WorldChrMan.InfinitePoise;
            memoryService.SetBitValue(infinitePoisePtr, flagMask, setValue);
        }

        public bool IsNoDeathOn()
        {
            var noDeathPtr = DebugFlags.Base + DebugFlags.NoDeath;
            return memoryService.ReadBytes(noDeathPtr, 1)[0] == 1;
        }

        public void ToggleInfiniteDurability(bool isInfiniteDurabilityEnabled)
        {
            if (isInfiniteDurabilityEnabled) memoryService.Write(Patches.InfiniteDurabilityPatch + 0x1, (byte)0x89);
            else memoryService.Write(Patches.InfiniteDurabilityPatch + 0x1, (byte)0x88);
        }
        
        public void ToggleNoRoll(bool isNoRollEnabled)
        {
            var noRollPatchPtr = Patches.NoRollPatch;
            var noBackstepPatchPtr = noRollPatchPtr + 0xFF;
            if (isNoRollEnabled)
            {
                memoryService.Write(noRollPatchPtr + 0x6, (byte)0);
                memoryService.Write(noRollPatchPtr + 0xD, (byte)0);
                memoryService.Write(noBackstepPatchPtr + 0x6, (byte)0);
                memoryService.Write(noBackstepPatchPtr + 0xD, (byte)0);
            }
            else
            {
                memoryService.Write(noRollPatchPtr + 0x6, (byte)1);
                memoryService.Write(noRollPatchPtr + 0xD, (byte)1);
                memoryService.Write(noBackstepPatchPtr + 0x6, (byte)1);
                memoryService.Write(noBackstepPatchPtr + 0xD, (byte)1);
            }
        }

        public int GetNewGame() =>
            memoryService.Read<int>(memoryService.Read<nint>(GameDataMan.Base) + (int)GameDataMan.GameDataOffsets.Ng);

        public void SetNewGame(int value) =>
            memoryService.Write(memoryService.Read<nint>(GameDataMan.Base) + (int)GameDataMan.GameDataOffsets.Ng,
                value);
        
        public float GetPlayerSpeed() => memoryService.Read<float>(GetPlayerSpeedPtr());

        public void SetPlayerSpeed(float speed) => memoryService.Write(GetPlayerSpeedPtr(), speed);

        private IntPtr GetPlayerSpeedPtr()
        {
            return memoryService.FollowPointers(memoryService.Read<nint>(WorldChrMan.Base),
                new[]
                {
                    (int)WorldChrMan.PlayerIns,
                    (int)WorldChrMan.PlayerInsOffsets.PlayerCtrl, WorldChrMan.ChrAnim,
                    WorldChrMan.ChrAnimSpeed
                }, false);
        }

        
        public void BreakWeapon(int slotOffset)
        {
            var playerGameData = memoryService.Read<nint>(memoryService.Read<nint>(GameDataMan.Base) +
                                                          (int)GameDataMan.GameDataOffsets.PlayerGameData);
            int equippedWep = memoryService.Read<int>(playerGameData + slotOffset);

            var equipGameData = memoryService.Read<nint>(playerGameData + (int)GameDataMan.PlayerGameData.EquipGameData);
            var bytes = AsmLoader.GetAsmBytes(AsmScript.BreakRightHandWep);
            AsmHelper.WriteAbsoluteAddresses(bytes, [
                (equipGameData, 0x0 + 2),
                (equippedWep, 0x12 + 2),
                (Funcs.GetInventoryIndexByCatAndId, 0x20 + 2)
            ]);
            
            memoryService.AllocateAndExecute(bytes);
        }
    }
    
    
}