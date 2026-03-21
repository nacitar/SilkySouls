// 

using System;
using System.Collections.Generic;
using System.Numerics;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services;

public class PlayerService(IMemoryService memoryService) : IPlayerService
{
    private readonly Dictionary<int, int> _lowLevelSoulRequirements = new()
    {
        { 2, 673 }, { 3, 690 }, { 4, 707 }, { 5, 724 }, { 6, 741 }, { 7, 758 }, { 8, 775 }, { 9, 793 }, { 10, 811 },
        { 11, 829 },
    };

    public int GetHp() => memoryService.Read<int>(GetPlayerIns() + ChrIns.Health);

    public int GetMaxHp() => memoryService.Read<int>(GetPlayerIns() + ChrIns.MaxHealth);

    public void SetHp(int hp) => memoryService.Write(GetPlayerIns() + ChrIns.Health, hp);

    public void SetRtsr() => SetHp(GetMaxHp() * 20 / 100 - 1);

    public void SetMaxHp() => SetHp(GetMaxHp());

    public int GetSp() => memoryService.Read<int>(GetPlayerIns() + ChrIns.Stamina);

    public void SetSp(int sp) => memoryService.Write(GetPlayerIns() + ChrIns.Stamina, sp);

    public Vector3 GetPosition() => memoryService.Read<Vector3>(GetPlayerIns() + ChrIns.Coords);

    public void RestoreSpellCasts()
    {
        var equipMagicData = memoryService.FollowPointers(memoryService.Read<nint>(GameDataMan.Base),
        [
            (int)GameDataMan.GameDataOffsets.PlayerGameData,
            (int)GameDataMan.PlayerGameData.EquipMagicData
        ], true);
        var bytes = AsmLoader.GetAsmBytes(AsmScript.RestoreSpellCasts);

        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (equipMagicData, 2),
            (RestoreCastsFunc, 0xE + 2)
        ]);

        memoryService.AllocateAndExecute(bytes);
    }

    public int GetPlayerStat(GameDataMan.PlayerGameData stat)
    {
        var statsBasePtr = memoryService.Read<nint>(memoryService.Read<nint>(GameDataMan.Base) +
                                                    (int)GameDataMan.GameDataOffsets.PlayerGameData);
        return memoryService.Read<int>(statsBasePtr + (int)stat);
    }

    public void SetPlayerStat(GameDataMan.PlayerGameData statType, int newValue)
    {
        var statPtr = memoryService.FollowPointers(memoryService.Read<nint>(GameDataMan.Base), [
            (int)GameDataMan.GameDataOffsets.PlayerGameData, (int)statType
        ], false);

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

    public void GiveSouls()
    {
        var soulsPtr = memoryService.FollowPointers(memoryService.Read<nint>(GameDataMan.Base), new[]
            {
                (int)GameDataMan.GameDataOffsets.PlayerGameData,
                (int)GameDataMan.PlayerGameData.Souls
            },
            false);
        int currentVal = memoryService.Read<int>(soulsPtr);
        HandleSoulEdit(soulsPtr, currentVal + 10000, currentVal);
    }

    public nint GetPlayerIns() =>
        memoryService.Read<nint>(memoryService.Read<nint>(WorldChrMan.Base) + WorldChrMan.PlayerIns);

    private void UpdatePlayerStats(int difference)
    {
        var allStatsPtr =
            memoryService.FollowPointers(memoryService.Read<nint>(GameDataMan.Base),
                [(int)GameDataMan.GameDataOffsets.PlayerGameData], true);

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

        byte[] codeBytes = AsmLoader.GetAsmBytes(AsmScript.LevelUp);
        byte[] bytes = BitConverter.GetBytes(statArrayAddress);
        Array.Copy(bytes, 0, codeBytes, 2, 8);
        bytes = BitConverter.GetBytes(soulsPtr);
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

    private void HandleSoulEdit(nint statPtr, int newValue, int oldValue)
    {
        if (newValue < oldValue)
        {
            memoryService.Write(statPtr, newValue);
            return;
        }

        int difference = newValue - oldValue;
        var totalSoulsPtr = memoryService.FollowPointers(memoryService.Read<nint>(GameDataMan.Base), new[]
            {
                (int)GameDataMan.GameDataOffsets.PlayerGameData,
                (int)GameDataMan.PlayerGameData.TotalSouls
            },
            false);
        int currentTotalSouls = memoryService.Read<int>(totalSoulsPtr);

        memoryService.Write(totalSoulsPtr, difference + currentTotalSouls);
        memoryService.Write(statPtr, newValue);
    }
}