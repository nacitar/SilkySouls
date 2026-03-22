using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using SilkySouls.Core;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Models;
using SilkySouls.Services;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.ViewModels
{
    public class PlayerViewModel : BaseViewModel
    {
        private readonly PlayerServiceOld _playerServiceOld;
        private readonly IPlayerService _playerService;
        private readonly HotkeyManager _hotkeyManager;
        private readonly IGameTickService _gameTickService;

        private CharacterState _saveState1 = new();
        private CharacterState _saveState2 = new();
        
        private float _playerDesiredSpeed = -1f;
        private const float DefaultSpeed = 1f;
        private const float Epsilon = 0.0001f;

        private bool _pauseUpdates;
        private bool _wasNoDamageEnabled;
        private bool _wasNoDeathEnabled;
        private int _currentSoulLevel;

        public PlayerViewModel(PlayerServiceOld playerServiceOld, IPlayerService playerService,
            HotkeyManager hotkeyManager, IStateService stateService, IGameTickService gameTickService)
        {
            _playerServiceOld = playerServiceOld;
            _playerService = playerService;
            _hotkeyManager = hotkeyManager;
            _gameTickService = gameTickService;

            stateService.Subscribe(State.Loaded, OnLoaded);
            stateService.Subscribe(State.NotLoaded, OnNotLoaded);

            SetRtsrCommand = new DelegateCommand(SetRtsr);
            SetMaxHpCommand = new DelegateCommand(SetMaxHp);
            SavePosCommand = new DelegateCommand(OnSavePos);
            RestorePosCommand = new DelegateCommand(OnRestorePos);
            RestoreSpellCastsCommand = new DelegateCommand(() => _playerService.RestoreSpellCasts());
            GiveSoulsCommand = new DelegateCommand(() => _playerService.GiveSouls());
            BreakWeaponCommand = new DelegateCommand(() => _playerServiceOld.BreakWeapon(SelectedWeaponSlot.SlotOffset));

            RegisterHotkeys();

            LoadStats();

            EquippedWeapons =
            [
                new("Right hand 1", 0x328),
                new("Right hand 2", 0x32C),
                new("Left hand 1", 0x324),
                new("Left hand 2", 0x330)
            ];
            SelectedWeaponSlot = EquippedWeapons.FirstOrDefault();
        }

        

        #region Commands

        public ICommand SetRtsrCommand { get; }
        public ICommand SetMaxHpCommand { get; }
        public ICommand SavePosCommand { get; }
        public ICommand RestorePosCommand { get; }
        public ICommand RestoreSpellCastsCommand { get; }
        public ICommand GiveSoulsCommand { get; }
        public ICommand BreakWeaponCommand { get; }

        #endregion

        #region Properties

        private bool _areOptionsEnabled;

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }

        private int _currentHp;

        public int CurrentHp
        {
            get => _currentHp;
            set => SetProperty(ref _currentHp, value);
        }

        private int _currentMaxHp;

        public int CurrentMaxHp
        {
            get => _currentMaxHp;
            set => SetProperty(ref _currentMaxHp, value);
        }

        private bool _isPos1Saved;

        public bool IsPos1Saved
        {
            get => _isPos1Saved;
            set => SetProperty(ref _isPos1Saved, value);
        }

        private bool _isPos2Saved;

        public bool IsPos2Saved
        {
            get => _isPos2Saved;
            set => SetProperty(ref _isPos2Saved, value);
        }

        private bool _isStateIncluded;

        public bool IsStateIncluded
        {
            get => _isStateIncluded;
            set => SetProperty(ref _isStateIncluded, value);
        }

        private float _posX;

        public float PosX
        {
            get => _posX;
            set => SetProperty(ref _posX, value);
        }

        private float _posY;

        public float PosY
        {
            get => _posY;
            set => SetProperty(ref _posY, value);
        }

        private float _posZ;

        public float PosZ
        {
            get => _posZ;
            set => SetProperty(ref _posZ, value);
        }

        private bool _isNoDeathEnabled;

        public bool IsNoDeathEnabled
        {
            get => _isNoDeathEnabled;
            set
            {
                if (SetProperty(ref _isNoDeathEnabled, value))
                {
                    _playerService.ToggleChrDebugFlag(DebugFlags.NoDeath, _isNoDeathEnabled);
                }
            }
        }

        private bool _isNoDamageEnabled;

        public bool IsNoDamageEnabled
        {
            get => _isNoDamageEnabled;
            set
            {
                if (SetProperty(ref _isNoDamageEnabled, value))
                {
                    _playerService.ToggleNoDamage(_isNoDamageEnabled);
                }
            }
        }

        private bool _isInfiniteStaminaEnabled;

        public bool IsInfiniteStaminaEnabled
        {
            get => _isInfiniteStaminaEnabled;
            set
            {
                if (SetProperty(ref _isInfiniteStaminaEnabled, value))
                {
                    _playerService.ToggleInfiniteStamina(_isInfiniteStaminaEnabled);
                }
            }
        }

        private bool _isNoGoodsConsumeEnabled;

        public bool IsNoGoodsConsumeEnabled
        {
            get => _isNoGoodsConsumeEnabled;
            set
            {
                if (SetProperty(ref _isNoGoodsConsumeEnabled, value))
                {
                    _playerService.ToggleNoGoodsConsume(_isNoGoodsConsumeEnabled);
                }
            }
        }

        private bool _isInfiniteCastsEnabled;

        public bool IsInfiniteCastsEnabled
        {
            get => _isInfiniteCastsEnabled;
            set
            {
                if (SetProperty(ref _isInfiniteCastsEnabled, value))
                {
                    _playerService.ToggleChrDebugFlag(DebugFlags.InfiniteCasts, _isInfiniteCastsEnabled);
                }
            }
        }

        private bool _isInfiniteDurabilityEnabled;

        public bool IsInfiniteDurabilityEnabled
        {
            get => _isInfiniteDurabilityEnabled;
            set
            {
                if (SetProperty(ref _isInfiniteDurabilityEnabled, value))
                {
                    _playerService.ToggleInfiniteDurability(_isInfiniteDurabilityEnabled);
                }
            }
        }

        private bool _isOneShotEnabled;

        public bool IsOneShotEnabled
        {
            get => _isOneShotEnabled;
            set
            {
                if (SetProperty(ref _isOneShotEnabled, value))
                {
                    _playerService.ToggleChrDebugFlag(DebugFlags.OneShot, _isOneShotEnabled);
                }
            }
        }

        private bool _isInvisibleEnabled;

        public bool IsInvisibleEnabled
        {
            get => _isInvisibleEnabled;
            set
            {
                if (SetProperty(ref _isInvisibleEnabled, value))
                {
                    _playerService.ToggleChrDebugFlag(DebugFlags.Invisible, _isInvisibleEnabled);
                }
            }
        }

        private bool _isSilentEnabled;

        public bool IsSilentEnabled
        {
            get => _isSilentEnabled;
            set
            {
                if (SetProperty(ref _isSilentEnabled, value))
                {
                    _playerService.ToggleChrDebugFlag(DebugFlags.Silent, _isSilentEnabled);
                }
            }
        }

        private bool _isNoAmmoConsumeEnabled;

        public bool IsNoAmmoConsumeEnabled
        {
            get => _isNoAmmoConsumeEnabled;
            set
            {
                if (SetProperty(ref _isNoAmmoConsumeEnabled, value))
                {
                    _playerService.ToggleChrDebugFlag(DebugFlags.NoAmmoConsume, _isNoAmmoConsumeEnabled);
                }
            }
        }

        private bool _isInfinitePoiseEnabled;

        public bool IsInfinitePoiseEnabled
        {
            get => _isInfinitePoiseEnabled;
            set
            {
                if (SetProperty(ref _isInfinitePoiseEnabled, value))
                {
                    _playerService.ToggleInfinitePoise(_isInfinitePoiseEnabled);
                }
            }
        }

        private bool _isAutoSetNewGameSixEnabled;

        public bool IsAutoSetNewGameSixEnabled
        {
            get => _isAutoSetNewGameSixEnabled;
            set => SetProperty(ref _isAutoSetNewGameSixEnabled, value);
        }

        private bool _isNoRollEnabled;

        public bool IsNoRollEnabled
        {
            get => _isNoRollEnabled;
            set
            {
                if (!SetProperty(ref _isNoRollEnabled, value)) return;
                _playerService.ToggleNoRoll(_isNoRollEnabled);
            }
        }

        private int _soulLevel;

        public int SoulLevel
        {
            get => _soulLevel;
            private set => SetProperty(ref _soulLevel, value);
        }

        private int _vitality;

        public int Vitality
        {
            get => _vitality;
            set => SetProperty(ref _vitality, value);
        }

        private int _attunement;

        public int Attunement
        {
            get => _attunement;
            set => SetProperty(ref _attunement, value);
        }

        private int _endurance;

        public int Endurance
        {
            get => _endurance;
            set => SetProperty(ref _endurance, value);
        }

        private int _strength;

        public int Strength
        {
            get => _strength;
            set => SetProperty(ref _strength, value);
        }

        private int _dexterity;

        public int Dexterity
        {
            get => _dexterity;
            set => SetProperty(ref _dexterity, value);
        }

        private int _resistance;

        public int Resistance
        {
            get => _resistance;
            set => SetProperty(ref _resistance, value);
        }

        private int _intelligence;

        public int Intelligence
        {
            get => _intelligence;
            set => SetProperty(ref _intelligence, value);
        }

        private int _faith;

        public int Faith
        {
            get => _faith;
            set => SetProperty(ref _faith, value);
        }

        private int _humanity;

        public int Humanity
        {
            get => _humanity;
            set => SetProperty(ref _humanity, value);
        }

        private int _souls;

        public int Souls
        {
            get => _souls;
            set => SetProperty(ref _souls, value);
        }

        private int _newGame;

        public int NewGame
        {
            get => _newGame;
            set
            {
                if (SetProperty(ref _newGame, value))
                {
                    _playerService.SetNewGame(value);
                }
            }
        }

        private float _playerSpeed;

        public float PlayerSpeed
        {
            get => _playerSpeed;
            set
            {
                if (SetProperty(ref _playerSpeed, value))
                {
                    _playerService.SetSpeed(value);
                }
            }
        }

        private List<EquippedWeapon> _equippedWeapons;

        public List<EquippedWeapon> EquippedWeapons
        {
            get => _equippedWeapons;
            set => SetProperty(ref _equippedWeapons, value);
        }

        private EquippedWeapon _selectedWeaponSlot;

        public EquippedWeapon SelectedWeaponSlot
        {
            get => _selectedWeaponSlot;
            set => SetProperty(ref _selectedWeaponSlot, value);
        }

        #endregion

        #region Public Methods

        public void PauseUpdates() => _pauseUpdates = true;
        public void ResumeUpdates() => _pauseUpdates = false;
        public void SetHp(int hp) => _playerService.SetHp(hp);

        public void SetStat(string statName, int val)
        {
            GameDataMan.PlayerGameData stat =
                (GameDataMan.PlayerGameData)Enum.Parse(typeof(GameDataMan.PlayerGameData), statName);
            _playerService.SetPlayerStat(stat, val);
        }

        public void TrySetNgPref()
        {
            if (IsAutoSetNewGameSixEnabled)
                _playerService.SetNewGame(7);
            NewGame = _playerService.GetNewGame();
        }

        #endregion

        #region Private Methods

        private void OnNotLoaded()
        {
            AreOptionsEnabled = false;
            _gameTickService.Unsubscribe(PlayerTick);
        }

        private void OnLoaded()
        {
            if (IsNoDeathEnabled) _playerService.ToggleChrDebugFlag(DebugFlags.NoDeath, true);
            if (IsNoDamageEnabled) _playerService.ToggleNoDamage(true);
            if (IsInfiniteStaminaEnabled) _playerService.ToggleInfiniteStamina(true);
            if (IsNoGoodsConsumeEnabled) _playerService.ToggleNoGoodsConsume(true);
            if (IsInfiniteCastsEnabled) _playerService.ToggleChrDebugFlag(DebugFlags.InfiniteCasts, true);
            if (IsOneShotEnabled) _playerService.ToggleChrDebugFlag(DebugFlags.OneShot, true);
            if (IsInvisibleEnabled) _playerService.ToggleChrDebugFlag(DebugFlags.Invisible, true);
            if (IsSilentEnabled) _playerService.ToggleChrDebugFlag(DebugFlags.Silent, true);
            if (IsNoAmmoConsumeEnabled) _playerService.ToggleChrDebugFlag(DebugFlags.NoAmmoConsume, true);
            if (IsInfinitePoiseEnabled) _playerService.ToggleInfinitePoise(true);
            if (IsInfiniteDurabilityEnabled) _playerService.ToggleInfiniteDurability(true);
            if (IsNoRollEnabled) _playerService.ToggleNoRoll(true);
            AreOptionsEnabled = true;
            LoadStats();
            _gameTickService.Subscribe(PlayerTick);
        }

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction(HotkeyActions.SavePos1, () => SavePos(0));
            _hotkeyManager.RegisterAction(HotkeyActions.SavePos2, () => SavePos(1));
            _hotkeyManager.RegisterAction(HotkeyActions.RestorePos1, () => RestorePos(0));
            _hotkeyManager.RegisterAction(HotkeyActions.RestorePos2, () => RestorePos(1));
            _hotkeyManager.RegisterAction(HotkeyActions.RTSR, SetRtsr);
            _hotkeyManager.RegisterAction(HotkeyActions.NoDeath, () => { IsNoDeathEnabled = !IsNoDeathEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.OneShot, () => { IsOneShotEnabled = !IsOneShotEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.RestoreSpellCasts, () =>
            {
                if (!AreOptionsEnabled) return;
                _playerService.RestoreSpellCasts();
            });
            _hotkeyManager.RegisterAction(HotkeyActions.ToggleSpeed, ToggleSpeed);
            _hotkeyManager.RegisterAction(HotkeyActions.IncreaseSpeed, () => SetSpeed(Math.Min(10, PlayerSpeed + 0.25f)));
            _hotkeyManager.RegisterAction(HotkeyActions.DecreaseSpeed, () => SetSpeed(Math.Max(0, PlayerSpeed - 0.25f)));
        }

        private void PlayerTick()
        {
            if (_pauseUpdates) return;

            CurrentHp = _playerService.GetHp();
            CurrentMaxHp = _playerService.GetMaxHp();
            Souls = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Souls);
            PlayerSpeed = _playerService.GetSpeed();
            var pos = _playerService.GetPosition();
            PosX = pos.X;
            PosY = pos.Z;
            PosZ = pos.Y;
            int newSoulLevel = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.SoulLevel);
            if (_currentSoulLevel != newSoulLevel)
            {
                SoulLevel = newSoulLevel;
                _currentSoulLevel = newSoulLevel;
                LoadStats();
            }
        }

        private void LoadStats()
        {
            Vitality = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Vitality);
            Attunement = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Attunement);
            Endurance = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Endurance);
            Strength = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Strength);
            Dexterity = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Dexterity);
            Resistance = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Resistance);
            Intelligence = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Intelligence);
            Faith = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Faith);
            Humanity = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Humanity);
            Souls = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Souls);
            NewGame = _playerService.GetNewGame();
            PlayerSpeed = _playerService.GetSpeed();
            SoulLevel = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.SoulLevel);
        }

        private void SavePos(int index)
        {
            var state = index == 0 ? _saveState1 : _saveState2;
            if (index == 0) IsPos1Saved = true;
            else IsPos2Saved = true;

            state.IncludesState = IsStateIncluded;
            if (IsStateIncluded)
            {
                state.Hp = CurrentHp;
                state.Sp = _playerService.GetSp();
            }

            _playerServiceOld.SavePos(index);
        }

        private void RestorePos(int index)
        {
            _wasNoDamageEnabled = IsNoDamageEnabled;
            _wasNoDeathEnabled = IsNoDeathEnabled;
            IsNoDamageEnabled = true;
            _isNoDeathEnabled = true;
            _playerServiceOld.RestorePos(index);
            IsNoDamageEnabled = _wasNoDamageEnabled;
            IsNoDeathEnabled = _wasNoDeathEnabled;
            if (!IsStateIncluded) return;

            var state = index == 0 ? _saveState1 : _saveState2;
            if (!IsStateIncluded || !state.IncludesState) return;
            _playerService.SetHp(state.Hp);
            _playerService.SetSp(state.Sp);
        }

        private void OnSavePos(object parameter)
        {
            int index = Convert.ToInt32(parameter);
            SavePos(index);
        }

        private void OnRestorePos(object parameter)
        {
            int index = Convert.ToInt32(parameter);
            RestorePos(index);
        }

        private void SetSpeed(float value)
        {
            PlayerSpeed = value;
        }

        private void ToggleSpeed()
        {
            if (!AreOptionsEnabled) return;

            if (!IsApproximately(PlayerSpeed, DefaultSpeed))
            {
                _playerDesiredSpeed = PlayerSpeed;
                SetSpeed(DefaultSpeed);
            }
            else if (_playerDesiredSpeed >= 0)
            {
                SetSpeed(_playerDesiredSpeed);
            }
        }

        private bool IsApproximately(float a, float b)
        {
            return Math.Abs(a - b) < Epsilon;
        }

        private void SetRtsr() => _playerService.SetRtsr();
        private void SetMaxHp() => _playerService.SetMaxHp();

        #endregion
    }
}
