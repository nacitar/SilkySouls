using System;
using System.Collections.Generic;
using System.Linq;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services;

public class PlayerHitBehaviorService : IPlayerHitBehaviorService
{
    private const int MaxTrackedEnemies = 256;
    private static readonly TimeSpan TrackedEnemyTtl = TimeSpan.FromSeconds(30);

    private readonly IMemoryService _memoryService;
    private readonly HookManager _hookManager;
    private readonly IPlayerService _playerService;
    private readonly ITargetService _targetService;
    private readonly Dictionary<nint, TrackedEnemyState> _trackedEnemies = new();
    private readonly List<nint> _hooks = [];

    private int _lastHitCount;
    private int _lastPlayerHp;
    private bool _isNegateNonFatalHitDamageEnabled;
    private bool _isHealEnemiesOnPlayerHitEnabled;
    private int _tickCounter;

    public bool IsNegateNonFatalHitDamageEnabled => _isNegateNonFatalHitDamageEnabled;
    public bool IsHealEnemiesOnPlayerHitEnabled => _isHealEnemiesOnPlayerHitEnabled;

    public PlayerHitBehaviorService(
        IMemoryService memoryService,
        HookManager hookManager,
        IPlayerService playerService,
        ITargetService targetService,
        IGameTickService gameTickService,
        IStateService stateService)
    {
        _memoryService = memoryService;
        _hookManager = hookManager;
        _playerService = playerService;
        _targetService = targetService;

        stateService.Subscribe(State.Loaded, OnLoaded);
        stateService.Subscribe(State.NotLoaded, OnNotLoaded);
        gameTickService.Subscribe(OnTick);
    }

    public void SetNegateNonFatalHitDamageEnabled(bool isEnabled)
    {
        if (_isNegateNonFatalHitDamageEnabled == isEnabled)
            return;

        _isNegateNonFatalHitDamageEnabled = isEnabled;
        if (isEnabled)
            InitializeOnEnable();
    }

    public void SetHealEnemiesOnPlayerHitEnabled(bool isEnabled)
    {
        if (_isHealEnemiesOnPlayerHitEnabled == isEnabled)
            return;

        _isHealEnemiesOnPlayerHitEnabled = isEnabled;
        if (isEnabled)
        {
            _trackedEnemies.Clear();
            InitializeOnEnable();
        }
    }

    private void InitializeOnEnable()
    {
        _lastHitCount = ReadHitCounterSafe();
        _lastPlayerHp = SafeGetPlayerHp();
        ClearLastAttackerPtr();
        TryEnableTargetHook();
        ForceReinstallHooks();
    }

    private void OnLoaded()
    {
        _lastHitCount = ReadHitCounter();
        _lastPlayerHp = SafeGetPlayerHp();
        ClearLastAttackerPtr();
        TryEnableTargetHook();
        _hooks.Clear();
        EnsureHooksInstalled();
    }

    private void OnNotLoaded()
    {
        _trackedEnemies.Clear();
        _lastHitCount = 0;
        _lastPlayerHp = 0;
    }

    private void OnTick()
    {
        if (_isNegateNonFatalHitDamageEnabled || _isHealEnemiesOnPlayerHitEnabled)
            EnsureHooksInstalled();

        TrackCurrentTarget();
        if (++_tickCounter % 120 == 0)
            PruneTrackedEnemies();

        if (!_isNegateNonFatalHitDamageEnabled && !_isHealEnemiesOnPlayerHitEnabled)
            return;

        bool hasHit = HasHit();
        bool hasLastAttacker = HasLastAttacker();
        bool isEnemyHit = hasHit && hasLastAttacker;

        if (isEnemyHit && !ShouldYieldToOverrides())
        {
            if (_isNegateNonFatalHitDamageEnabled)
                RestorePlayerHp();

            if (_isHealEnemiesOnPlayerHitEnabled)
            {
                RestoreLastAttacker();
                RestoreCurrentTarget();
                RestoreTrackedEnemies();
            }
        }

        int currentPlayerHp = SafeGetPlayerHp();
        if (currentPlayerHp > 0)
            _lastPlayerHp = currentPlayerHp;
    }

    private void TrackCurrentTarget()
    {
        if (!TryGetTarget(out var target, out var hp, out var maxHp))
            return;

        if (_trackedEnemies.Count >= MaxTrackedEnemies && !_trackedEnemies.ContainsKey(target))
            PruneTrackedEnemies();

        if (!_trackedEnemies.TryGetValue(target, out var state))
        {
            _trackedEnemies[target] = new TrackedEnemyState(hp, maxHp);
            return;
        }

        state.MaxHp = Math.Max(state.MaxHp, hp);
        state.GameMaxHp = Math.Max(state.GameMaxHp, maxHp);
        state.LastSeenUtc = DateTime.UtcNow;
    }

    private void RestoreTrackedEnemies()
    {
        foreach (var pair in _trackedEnemies)
        {
            var state = pair.Value;
            if (state.MaxHp <= 1)
                continue;
            try
            {
                var currentHp = _memoryService.Read<int>(pair.Key + ChrIns.Health);
                if (currentHp > 0)
                    _memoryService.Write(pair.Key + ChrIns.Health, Math.Min(state.MaxHp, state.GameMaxHp));
            }
            catch
            {
            }
        }
    }

    private void RestoreCurrentTarget()
    {
        if (!TryGetTarget(out var target, out var hp, out var maxHp))
            return;

        RestoreEnemyHp(target, hp, maxHp);
    }

    private void RestoreLastAttacker()
    {
        try
        {
            var attackerPtrAddr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.LastAttackerPtr;
            var attacker = _memoryService.Read<nint>(attackerPtrAddr);
            if (attacker == nint.Zero)
                return;

            var hp = _memoryService.Read<int>(attacker + ChrIns.Health);
            var maxHp = _memoryService.Read<int>(attacker + ChrIns.MaxHealth);
            if (hp <= 0 || maxHp <= 1)
                return;

            RestoreEnemyHp(attacker, hp, maxHp);

            _memoryService.Write(attackerPtrAddr, nint.Zero);
        }
        catch
        {
        }
    }

    private bool TryGetTarget(out nint target, out int hp, out int maxHp)
    {
        target = nint.Zero;
        hp = 0;
        maxHp = 0;

        try
        {
            target = _memoryService.Read<nint>(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr);
            if (target == nint.Zero)
                return false;

            hp = _memoryService.Read<int>(target + ChrIns.Health);
            maxHp = _memoryService.Read<int>(target + ChrIns.MaxHealth);
            return hp > 0 && maxHp > 1;
        }
        catch
        {
            return false;
        }
    }

    private void RestoreEnemyHp(nint enemy, int hp, int maxHp)
    {
        var state = GetOrUpdateTrackedEnemyState(enemy, hp, maxHp);
        _memoryService.Write(enemy + ChrIns.Health, Math.Min(state.MaxHp, state.GameMaxHp));
    }

    private TrackedEnemyState GetOrUpdateTrackedEnemyState(nint enemy, int hp, int maxHp)
    {
        if (_trackedEnemies.TryGetValue(enemy, out var state))
        {
            state.MaxHp = Math.Max(state.MaxHp, hp);
            state.GameMaxHp = Math.Max(state.GameMaxHp, maxHp);
            state.LastSeenUtc = DateTime.UtcNow;
            return state;
        }

        var newState = new TrackedEnemyState(hp, maxHp);
        _trackedEnemies[enemy] = newState;
        return newState;
    }

    private void PruneTrackedEnemies()
    {
        if (_trackedEnemies.Count == 0)
            return;

        var now = DateTime.UtcNow;
        var toRemove = new List<nint>();

        foreach (var pair in _trackedEnemies)
        {
            if (now - pair.Value.LastSeenUtc > TrackedEnemyTtl)
            {
                toRemove.Add(pair.Key);
                continue;
            }

            try
            {
                if (_memoryService.Read<int>(pair.Key + ChrIns.Health) <= 0)
                    toRemove.Add(pair.Key);
            }
            catch
            {
                toRemove.Add(pair.Key);
            }
        }

        foreach (var enemy in toRemove)
            _trackedEnemies.Remove(enemy);

        if (_trackedEnemies.Count <= MaxTrackedEnemies)
            return;

        foreach (var enemy in _trackedEnemies.OrderBy(x => x.Value.LastSeenUtc).Take(_trackedEnemies.Count - MaxTrackedEnemies).Select(x => x.Key).ToList())
            _trackedEnemies.Remove(enemy);
    }

    private void ClearLastAttackerPtr()
    {
        try
        {
            _memoryService.Write(CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.LastAttackerPtr, nint.Zero);
        }
        catch
        {
        }
    }

    private bool HasLastAttacker()
    {
        try
        {
            var attackerPtrAddr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.LastAttackerPtr;
            return _memoryService.Read<nint>(attackerPtrAddr) != nint.Zero;
        }
        catch
        {
            return false;
        }
    }

    private void TryEnableTargetHook()
    {
        try
        {
            _targetService.ToggleTargetHook(true);
        }
        catch
        {
        }
    }

    private void RestorePlayerHp()
    {
        if (_lastPlayerHp <= 0)
            return;
        try
        {
            _playerService.SetHp(_lastPlayerHp);
        }
        catch
        {
        }
    }

    private bool ShouldYieldToOverrides()
    {
        try
        {
            var playerIns = _playerService.GetPlayerIns();
            bool noDamage = _memoryService.IsBitSet(playerIns + ChrIns.NoDamage.Offset, ChrIns.NoDamage.Bit);
            bool allNoDamage = _memoryService.Read<byte>(DebugFlags.Base + DebugFlags.AllNoDamage) != 0;
            return noDamage || allNoDamage;
        }
        catch
        {
            return false;
        }
    }

    private bool HasHit()
    {
        var current = ReadHitCounter();
        var hasHit = current > _lastHitCount;
        _lastHitCount = current;
        return hasHit;
    }

    private int ReadHitCounter()
    {
        return _memoryService.Read<int>(CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.HitCounter);
    }

    private int ReadHitCounterSafe()
    {
        try
        {
            return ReadHitCounter();
        }
        catch
        {
            return 0;
        }
    }

    private int SafeGetPlayerHp()
    {
        try
        {
            return _playerService.GetHp();
        }
        catch
        {
            return 0;
        }
    }

    private void EnsureHooksInstalled()
    {
        if (_hooks.Any(h => _memoryService.Read<byte>(h) != 0xE9))
        {
            _hooks.Clear();
            InstallHooks();
        }
        else if (_hooks.Count == 0)
        {
            InstallHooks();
        }
    }

    private void ForceReinstallHooks()
    {
        _hooks.Clear();
        try
        {
            InstallHooks();
        }
        catch
        {
        }
    }

    private void InstallHooks()
    {
        InstallHitHook();
        InstallApplyHealthDeltaHook();
        InstallKillChrHook();
        InstallCheckAuxAttacker();
        InstallAuxProcHook();
        InstallClearThrowStateHook();
        InstallSetThrowStateHook();
    }

    private void InstallHook(nint code, nint hookAddr, byte[] originalBytes)
    {
        _hookManager.InstallHook(code, hookAddr, originalBytes);
        _hooks.Add(hookAddr);
    }

    private void InstallHitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRHit);
        var hit = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.HitCounter;
        var lastAttacker = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.LastAttackerPtr;
        var envDeathFlag = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.CheckEnvDeathFlag;
        var throwFlag = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.InThrowFlag;
        var code = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.HitCode;
        var originalBytes = GetHitOriginalBytes();

        Array.Copy(originalBytes, 0, bytes, 0xAB, originalBytes.Length);
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, envDeathFlag, 7, 2),
            (code + 0x7, throwFlag, 7, 0x7 + 2),
            (code + 0x15, WorldChrMan.Base, 7, 0x15 + 3),
            (code + 0x46, lastAttacker, 7, 0x46 + 3),
            (code + 0x9B, hit, 6, 0x9B + 2),
            (code + 0xA3, envDeathFlag, 7, 0xA3 + 2),
            (code + 0xB0, Hooks.Hit + 5, 5, 0xB0 + 1),
        ]);

        _memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.Hit, originalBytes);
    }

    private void InstallApplyHealthDeltaHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRApplyHealthDelta);
        var hit = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.HitCounter;
        var envDeathFlag = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.CheckEnvDeathFlag;
        var code = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.ApplyHealthDeltaCode;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x17, envDeathFlag, 7, 0x17 + 2),
            (code + 0x42, WorldChrMan.Base, 7, 0x42 + 3),
            (code + 0x54, hit, 6, 0x54 + 2),
            (code + 0x5B, Hooks.ApplyHealthDelta + 5, 5, 0x5B + 1),
        ]);

        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (FallDmgRetAddr, 0x6 + 2),
            (EnvDeathRetAddr, 0x20 + 2),
            (AuxDeathRetAddr, 0x31 + 2),
        ]);

        _memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.ApplyHealthDelta, [0x48, 0x89, 0x7C, 0x24, 0x40]);
    }

    private void InstallKillChrHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRKillChr);
        var hit = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.HitCounter;
        var code = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.KillChrCode;
        var originalBytes = GetKillChrOriginalBytes();

        Array.Copy(originalBytes, 0, bytes, 0, originalBytes.Length);
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x6, WorldChrMan.Base, 7, 0x6 + 3),
            (code + 0x18, hit, 6, 0x18 + 2),
            (code + 0x1F, Hooks.KillChr + 5, 5, 0x1F + 1),
        ]);

        _memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.KillChr, originalBytes);
    }

    private void InstallCheckAuxAttacker()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRCheckAuxAttacker);
        var code = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.CheckAuxAttackerCode;
        var checkAuxProcFlag = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.CheckAuxProcFlag;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, checkAuxProcFlag, 7, 2),
            (code + 0x17, WorldChrMan.Base, 7, 0x17 + 3),
            (code + 0x4B, checkAuxProcFlag, 7, 0x4B + 2),
            (code + 0x53, Hooks.CheckAuxAttacker + 7, 5, 0x53 + 1),
        ]);

        _memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.CheckAuxAttacker, [0x0F, 0xB6, 0x80, 0x56, 0x01, 0x00, 0x00]);
    }

    private void InstallAuxProcHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRAuxProc);
        var code = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.CheckAuxProcCode;
        var hit = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.HitCounter;
        var checkAuxProcFlag = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.CheckAuxProcFlag;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, checkAuxProcFlag, 7, 0x7 + 2),
            (code + 0x11, WorldChrMan.Base, 7, 0x11 + 3),
            (code + 0x34, hit, 6, 0x34 + 2),
            (code + 0x3B, Hooks.CheckAuxProc + 7, 5, 0x3B + 1),
        ]);

        _memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.CheckAuxProc, [0x44, 0x8B, 0x83, 0x34, 0x04, 0x00, 0x00]);
    }

    private void InstallClearThrowStateHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRClearThrowState);
        var code = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.ClearThrowStateCode;
        var throwFlag = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.InThrowFlag;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0xD, WorldChrMan.Base, 7, 0xD + 3),
            (code + 0x25, throwFlag, 7, 0x25 + 2),
            (code + 0x2D, Hooks.ClearThrowState + 7, 5, 0x2D + 1),
        ]);

        _memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.ClearThrowState, [0x48, 0x8B, 0x8B, 0x48, 0x04, 0x00, 0x00]);
    }

    private void InstallSetThrowStateHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRSetThrowState);
        var code = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.SetThrowStateCode;
        var throwFlag = CodeCaveOffsets.Base + (int)CodeCaveOffsets.HitlessPractice.InThrowFlag;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x2D, WorldChrMan.Base, 7, 0x2D + 3),
            (code + 0x47, throwFlag, 7, 0x47 + 2),
            (code + 0x50, Hooks.SetThrowState + 10, 5, 0x50 + 1),
        ]);

        _memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.SetThrowState, [0x81, 0x8B, 0xDC, 0x01, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00]);
    }

    private static byte[] GetKillChrOriginalBytes()
    {
        return SilkySouls.memory.Offsets.Version switch
        {
            GameVersion.Version1_0_1_0 or GameVersion.Version1_0_1_1 or GameVersion.Version1_0_3_0 => [0x48, 0x8D, 0x64, 0x24, 0xF8],
            GameVersion.Version1_0_1_2 or GameVersion.Version1_0_3_1 => [0x48, 0x89, 0x5C, 0x24, 0xF8],
            _ => [0x48, 0x8D, 0x64, 0x24, 0xF8],
        };
    }

    private static byte[] GetHitOriginalBytes()
    {
        return SilkySouls.memory.Offsets.Version switch
        {
            GameVersion.Version1_0_1_0 or GameVersion.Version1_0_1_1 or GameVersion.Version1_0_3_0 => [0x48, 0x89, 0x6C, 0x24, 0x10],
            GameVersion.Version1_0_1_2 or GameVersion.Version1_0_3_1 => [0x48, 0x89, 0x5C, 0x24, 0x08],
            _ => [0x48, 0x89, 0x6C, 0x24, 0x10],
        };
    }

    private sealed class TrackedEnemyState
    {
        public TrackedEnemyState(int initialObservedHp, int gameMaxHp)
        {
            MaxHp = initialObservedHp;
            GameMaxHp = gameMaxHp;
            LastSeenUtc = DateTime.UtcNow;
        }

        public int MaxHp { get; set; }
        public int GameMaxHp { get; set; }
        public DateTime LastSeenUtc { get; set; }
    }
}
