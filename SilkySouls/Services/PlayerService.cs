using System;
using System.Collections.Generic;
using System.Threading;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services
{
    public class PlayerService(IMemoryService memoryService)
    {

        private readonly Dictionary<int, int> _lowLevelSoulRequirements = new()
        {
            { 2, 673 }, { 3, 690 }, { 4, 707 }, { 5, 724 }, { 6, 741 }, { 7, 758 }, { 8, 775 }, { 9, 793 }, { 10, 811 },
            { 11, 829 },
        };

        public int GetPlayerStat(GameDataMan.PlayerGameData stat)
        {
            var statsBasePtr = memoryService.Read<nint>(memoryService.Read<nint>(GameDataMan.Base) +
                                                           (int)GameDataMan.GameDataOffsets.PlayerGameData);
            return memoryService.Read<int>(statsBasePtr + (int)stat);
        }

        public void SetPlayerStat(GameDataMan.PlayerGameData statType, int newValue)
        {
            var statPtr = memoryService.FollowPointers(GameDataMan.Base, new[]
                { (int)GameDataMan.GameDataOffsets.PlayerGameData, (int)statType }, false);

            int currentValue = memoryService.Read<int>(statPtr);
            if (currentValue == newValue) return;

            switch (statType)
            {
                case GameDataMan.PlayerGameData.Souls:
                    HandleSoulEdit(statPtr, newValue, currentValue);
                    return;
                case GameDataMan.PlayerGameData.Humanity:
                    var validatedHumanity = newValue;
                    if (validatedHumanity < 1) validatedHumanity = 1;
                    if (validatedHumanity > 99) validatedHumanity = 99;
                    memoryService.Write(statPtr, validatedHumanity);
                    return;

                default:
                    var validatedStat = newValue;
                    if (validatedStat < 1) validatedStat = 1;
                    if (validatedStat > 99) validatedStat = 99;
                    if (validatedStat != currentValue)
                    {
                        memoryService.Write(statPtr, validatedStat);
                        UpdatePlayerStats(validatedStat - currentValue);
                    }

                    return;
            }
        }

        private void HandleSoulEdit(IntPtr statPtr, int newValue, int oldValue)
        {
            if (newValue < oldValue)
            {
                memoryService.Write(statPtr, newValue);
                return;
            }

            int difference = newValue - oldValue;
            var totalSoulsPtr = memoryService.FollowPointers(GameDataMan.Base, new[]
                {
                    (int)GameDataMan.GameDataOffsets.PlayerGameData,
                    (int)GameDataMan.PlayerGameData.TotalSouls
                },
                false);
            int currentTotalSouls = memoryService.Read<int>(totalSoulsPtr);

            memoryService.Write(totalSoulsPtr, difference + currentTotalSouls);
            memoryService.Write(statPtr, newValue);
        }

        private void UpdatePlayerStats(int difference)
        {
            var allStatsPtr =
                memoryService.FollowPointers(GameDataMan.Base,
                    new[] { (int)GameDataMan.GameDataOffsets.PlayerGameData }, true);

            int originalSouls = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Souls);

            int currentLevel = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.SoulLevel);
            int newLevel = currentLevel + difference;


            int[] stats = new int[9];
            stats[0] = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Vitality);
            stats[1] = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Attunement);
            stats[2] = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Endurance);
            stats[3] = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Strength);
            stats[4] = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Dexterity);
            stats[5] = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Resistance);
            stats[6] = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Intelligence);
            stats[7] = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Faith);
            stats[8] = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Humanity);

            if (CallLevelUpFunction(newLevel, stats))
            {
                if (newLevel < currentLevel)
                {
                    memoryService.Write(allStatsPtr + (int)GameDataMan.PlayerGameData.Souls, originalSouls);
                    return;
                }

                int totalSoulsRequired = CalculateTotalSoulsRequired(currentLevel, newLevel);
                int currentTotalSouls =
                    memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.TotalSouls);
                memoryService.Write(allStatsPtr + (int)GameDataMan.PlayerGameData.TotalSouls,
                    totalSoulsRequired + currentTotalSouls);
                memoryService.Write(allStatsPtr + (int)GameDataMan.PlayerGameData.Souls, originalSouls);
            }
        }

        private bool CallLevelUpFunction(int newLevel, int[] stats)
        {
            var statArrayAddress = CodeCaveOffsets.Base + (int)CodeCaveOffsets.LevelUp.StatsArray;
            var tempStatArray = statArrayAddress;
            for (int i = 0; i < 9; i++)
            {
                memoryService.Write(tempStatArray, stats[i]);
                tempStatArray += 0x4;
            }

            var codeStart = CodeCaveOffsets.Base + (int)CodeCaveOffsets.LevelUp.CodeBlock;
            var soulsPtr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.LevelUp.SoulsPtr;

            byte[] codeBytes = AsmLoader.GetAsmBytes("LevelUp");
            byte[] bytes = BitConverter.GetBytes(statArrayAddress.ToInt64());
            Array.Copy(bytes, 0, codeBytes, 2, 8);
            bytes = BitConverter.GetBytes(soulsPtr.ToInt64());
            Array.Copy(bytes, 0, codeBytes, 15, 8);
            bytes = BitConverter.GetBytes(LevelUpFunc);
            Array.Copy(bytes, 0, codeBytes, 32, 8);
            memoryService.WriteBytes(codeStart, codeBytes);

            var newLevelAddr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.LevelUp.NewLevel;
            memoryService.Write(newLevelAddr, newLevel);
            var requiredSoulsAddr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.LevelUp.RequiredSouls;
            memoryService.Write(requiredSoulsAddr, 0);

            var currSoulsAddr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.LevelUp.CurrentSouls;
            memoryService.Write(currSoulsAddr, 9999999);

            return memoryService.RunThreadAndWaitForCompletion(codeStart);
        }

        private int CalculateTotalSoulsRequired(int startLevel, int endLevel)
        {
            startLevel = Math.Max(1, startLevel);
            double totalSouls = 0;
            for (int level = startLevel + 1; level <= endLevel; level++)
            {
                if (level <= 11)
                {
                    totalSouls += _lowLevelSoulRequirements[level];
                }
                else
                {
                    double x = level;
                    double levelCost = 0.02 * Math.Pow(x, 3) + 3.06 * Math.Pow(x, 2) + 105.6 * x - 895;
                    totalSouls += Math.Round(levelCost);
                }
            }

            return (int)totalSouls;
        }

        public void SetHp(int hp) =>
            memoryService.Write(GetPlayerInsPointer((int)WorldChrMan.PlayerInsOffsets.Health), hp);


        public int GetHp() =>
            memoryService.Read<int>(GetPlayerInsPointer((int)WorldChrMan.PlayerInsOffsets.Health));

        public int GetMaxHp() =>
            memoryService.Read<int>(GetPlayerInsPointer((int)WorldChrMan.PlayerInsOffsets.MaxHealth));

        public IntPtr GetPlayerInsPointer(int finalOffset)
        {
            var ptr = memoryService.FollowPointers(WorldChrMan.Base,
                new[]
                {
                    (int)WorldChrMan.BaseOffsets.PlayerIns,
                    finalOffset
                }, false);

            return ptr;
        }

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
            return memoryService.FollowPointers(WorldChrMan.Base, new[]
            {
                (int)WorldChrMan.BaseOffsets.PlayerIns,
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

        public void RestoreSpellCasts()
        {
            var magicDataPtr = memoryService.FollowPointers(GameDataMan.Base,
                new[]
                {
                    (int)GameDataMan.GameDataOffsets.PlayerGameData,
                    (int)GameDataMan.PlayerGameData.EquipMagicData
                }, true);
            byte[] restoreBytes = AsmLoader.GetAsmBytes("RestoreSpellCasts");
            byte[] bytes = BitConverter.GetBytes(magicDataPtr);
            Array.Copy(bytes, 0, restoreBytes, 2, 8);
            bytes = BitConverter.GetBytes(RestoreCastsFunc);
            Array.Copy(bytes, 0, restoreBytes, 16, 8);
            memoryService.AllocateAndExecute(restoreBytes);
        }

        public void ToggleNoDeath(int value)
        {
            var noDeathPtr = DebugFlags.Base + DebugFlags.NoDeath;
            memoryService.Write(noDeathPtr, (byte)value);
        }


        public void ToggleNoDamage(bool setValue)
        {
            var noDamagePtr = memoryService.FollowPointers(WorldChrMan.Base,
                new[]
                {
                    (int)WorldChrMan.BaseOffsets.PlayerIns, (int)WorldChrMan.PlayerInsOffsets.NoDamage
                }, false);
            var flagMask = WorldChrMan.NoDamage;
            memoryService.SetBitValue(noDamagePtr, flagMask, setValue);
        }

        public void ToggleInfiniteStamina(bool setValue)
        {
            var infiniteStamPtr = memoryService.FollowPointers(WorldChrMan.Base,
                new[]
                {
                    (int)WorldChrMan.BaseOffsets.PlayerIns,
                    (int)WorldChrMan.PlayerInsOffsets.ChrFlags
                }, false);

            var flagMask = (byte)WorldChrMan.ChrFlags.InfiniteStam;
            memoryService.SetBitValue(infiniteStamPtr, flagMask, setValue);
        }

        public void ToggleNoGoodsConsume(bool setValue)
        {
            var noGoodsConsumePtr = memoryService.FollowPointers(WorldChrMan.Base,
                new[]
                {
                    (int)WorldChrMan.BaseOffsets.PlayerIns,
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
            var infinitePoisePtr = memoryService.FollowPointers(WorldChrMan.Base,
                new[]
                {
                    (int)WorldChrMan.BaseOffsets.PlayerIns,
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

        public int GetSp() =>
            memoryService.Read<int>(GetPlayerInsPointer((int)WorldChrMan.PlayerInsOffsets.Stamina));

        public void SetSp(int sp) =>
            memoryService.Write(GetPlayerInsPointer((int)WorldChrMan.PlayerInsOffsets.Stamina), sp);

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

        public void GiveSouls()
        {
            var soulsPtr = memoryService.FollowPointers(GameDataMan.Base, new[]
                {
                    (int)GameDataMan.GameDataOffsets.PlayerGameData,
                    (int)GameDataMan.PlayerGameData.Souls
                },
                false);
            int currentVal = memoryService.Read<int>(soulsPtr);
            HandleSoulEdit(soulsPtr, currentVal + 10000, currentVal);
        }

        public float GetPlayerSpeed() => memoryService.Read<float>(GetPlayerSpeedPtr());

        public void SetPlayerSpeed(float speed) => memoryService.Write(GetPlayerSpeedPtr(), speed);

        private IntPtr GetPlayerSpeedPtr()
        {
            return memoryService.FollowPointers(WorldChrMan.Base,
                new[]
                {
                    (int)WorldChrMan.BaseOffsets.PlayerIns,
                    (int)WorldChrMan.PlayerInsOffsets.PlayerCtrl, WorldChrMan.ChrAnim,
                    WorldChrMan.ChrAnimSpeed
                }, false);
        }

        public (float x, float y, float z) GetReadOnlyCoords()
        {
            var playerInsPtr = memoryService.Read<nint>(memoryService.Read<nint>(WorldChrMan.Base) +
                                                           (int)WorldChrMan.BaseOffsets.PlayerIns);

            var coordBytes = memoryService.ReadBytes(playerInsPtr + (int)WorldChrMan.PlayerInsOffsets.ReadOnlyCoords, 12);
            float x = BitConverter.ToSingle(coordBytes, 0);
            float z = BitConverter.ToSingle(coordBytes, 4);
            float y = BitConverter.ToSingle(coordBytes, 8);
            return (x, y, z); 
        }

        public void SetAxis(WorldChrMan.Coords coords, float value) =>
            memoryService.Write(GetPlayerCoordinatePtr(coords), value);

        public void BreakWeapon(int slotOffset)
        {
            var playerGameData = memoryService.Read<nint>(memoryService.Read<nint>(GameDataMan.Base) +
                                                          (int)GameDataMan.GameDataOffsets.PlayerGameData);
            int equippedWep = memoryService.Read<int>(playerGameData + slotOffset);

            var equipGameData = memoryService.Read<nint>(playerGameData + (int)GameDataMan.PlayerGameData.EquipGameData);
            var bytes = AsmLoader.GetAsmBytes("BreakRightHandWep");
            AsmHelper.WriteAbsoluteAddresses64(bytes, new []
            {
                (equipGameData, 0x0 + 2),
                (equippedWep, 0x12 + 2),
                (Funcs.GetInventoryIndexByCatAndId, 0x20 + 2)
            });
            
            memoryService.AllocateAndExecute(bytes);
        }
    }
    
    
}