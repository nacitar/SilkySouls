// 

using System;
using System.Collections.ObjectModel;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Utilities;
using SilkySouls.Views;

namespace SilkySouls.ViewModels;

public class TargetViewModel : BaseViewModel
{
    private readonly IGameTickService _gameTickService;
    private readonly HotkeyManager _hotkeyManager;
    private readonly ITargetService _targetService;

    private nint _currentChrIns;
    private float _targetDesiredSpeed = -1f;
    private ResistancesWindow _resistancesWindowWindow;

    private string _currentlyRepeatingAct = "None";

    private const float DefaultSpeed = 1f;
    private const float Epsilon = 0.0001f;

    public TargetViewModel(ITargetService targetService, HotkeyManager hotkeyManager, IGameTickService gameTickService,
        IStateService stateService)
    {
        _targetService = targetService;
        _gameTickService = gameTickService;

        stateService.Subscribe(State.Loaded, OnLoaded);
        stateService.Subscribe(State.NotLoaded, OnNotLoaded);

        _repeatActOptions = new ObservableCollection<string> { "None" };
        SelectedRepeatActOption = "None";

        RegisterHotkeys();
    }

    #region Commands

    #endregion

    #region Properties

    private bool _areOptionsEnabled;

    public bool AreOptionsEnabled
    {
        get => _areOptionsEnabled;
        set => SetProperty(ref _areOptionsEnabled, value);
    }

    private bool _isTargetOptionsEnabled;

    public bool IsTargetOptionsEnabled
    {
        get => _isTargetOptionsEnabled;
        set
        {
            if (!SetProperty(ref _isTargetOptionsEnabled, value)) return;
            if (value)
            {
                _targetService.ToggleTargetHook(true);
                _gameTickService.Subscribe(TargetTick);
                ShowAllResistances = true;
            }
            else
            {
                _gameTickService.Unsubscribe(TargetTick);
                IsRepeatActEnabled = false;
                ShowAllResistances = false;
                IsResistancesWindowOpen = false;
                IsFreezeHealthEnabled = false;
                _targetService.ToggleTargetHook(false);
                ShowPoise = false;
                ShowBleed = false;
                ShowPoison = false;
                ShowToxic = false;
            }
        }
    }

    private bool _isValidTarget;

    public bool IsValidTarget
    {
        get => _isValidTarget;
        set => SetProperty(ref _isValidTarget, value);
    }

    private int _currentHealth;

    public int CurrentHealth
    {
        get => _currentHealth;
        set => SetProperty(ref _currentHealth, value);
    }

    private int _maxHealth;

    public int MaxHealth
    {
        get => _maxHealth;
        set => SetProperty(ref _maxHealth, value);
    }

    private float _targetCurrentPoise;

    public float TargetCurrentPoise
    {
        get => _targetCurrentPoise;
        set => SetProperty(ref _targetCurrentPoise, value);
    }

    private float _targetMaxPoise;

    public float TargetMaxPoise
    {
        get => _targetMaxPoise;
        set => SetProperty(ref _targetMaxPoise, value);
    }

    private float _targetPoiseTimer;

    public float TargetPoiseTimer
    {
        get => _targetPoiseTimer;
        set => SetProperty(ref _targetPoiseTimer, value);
    }

    private bool _showPoise;

    public bool ShowPoise
    {
        get => _showPoise;
        set => SetProperty(ref _showPoise, value);
    }

    private int _targetCurrentBleed;

    public int TargetCurrentBleed
    {
        get => _targetCurrentBleed;
        set => SetProperty(ref _targetCurrentBleed, value);
    }

    private int _targetMaxBleed;

    public int TargetMaxBleed
    {
        get => _targetMaxBleed;
        set => SetProperty(ref _targetMaxBleed, value);
    }

    private bool _showBleed;

    public bool ShowBleed
    {
        get => _showBleed;
        set => SetProperty(ref _showBleed, value);
    }

    private bool _isBleedImmune;

    public bool IsBleedImmune
    {
        get => _isBleedImmune;
        set => SetProperty(ref _isBleedImmune, value);
    }

    private int _targetCurrentPoison;

    public int TargetCurrentPoison
    {
        get => _targetCurrentPoison;
        set => SetProperty(ref _targetCurrentPoison, value);
    }

    private int _targetMaxPoison;

    public int TargetMaxPoison
    {
        get => _targetMaxPoison;
        set => SetProperty(ref _targetMaxPoison, value);
    }

    private bool _showPoison;

    public bool ShowPoison
    {
        get => _showPoison;
        set => SetProperty(ref _showPoison, value);
    }

    private bool _isPoisonImmune;

    public bool IsPoisonImmune
    {
        get => _isPoisonImmune;
        set => SetProperty(ref _isPoisonImmune, value);
    }

    private int _targetCurrentToxic;

    public int TargetCurrentToxic
    {
        get => _targetCurrentToxic;
        set => SetProperty(ref _targetCurrentToxic, value);
    }

    private int _targetMaxToxic;

    public int TargetMaxToxic
    {
        get => _targetMaxToxic;
        set => SetProperty(ref _targetMaxToxic, value);
    }

    private bool _showToxic;

    public bool ShowToxic
    {
        get => _showToxic;
        set => SetProperty(ref _showToxic, value);
    }

    private bool _isToxicImmune;

    public bool IsToxicImmune
    {
        get => _isToxicImmune;
        set => SetProperty(ref _isToxicImmune, value);
    }

    private float _targetSpeed;

    public float TargetSpeed
    {
        get => _targetSpeed;
        set
        {
            if (SetProperty(ref _targetSpeed, value))
            {
                _targetService.SetSpeed(value);
            }
        }
    }

    private bool _isFreezeHealthEnabled;

    public bool IsFreezeHealthEnabled
    {
        get => _isFreezeHealthEnabled;
        set
        {
            SetProperty(ref _isFreezeHealthEnabled, value);
            _targetService.ToggleNoDamage(_isFreezeHealthEnabled);
        }
    }

    private bool _isDisableTargetAiEnabled;

    public bool IsDisableTargetAiEnabled
    {
        get => _isDisableTargetAiEnabled;
        set
        {
            if (SetProperty(ref _isDisableTargetAiEnabled, value))
            {
                _targetService.ToggleAi(_isDisableTargetAiEnabled);
            }
        }
    }

    private ObservableCollection<string> _repeatActOptions;

    public ObservableCollection<string> RepeatActOptions
    {
        get => _repeatActOptions;
        set => SetProperty(ref _repeatActOptions, value);
    }

    private string _selectedRepeatActOption;

    public string SelectedRepeatActOption
    {
        get => _selectedRepeatActOption;
        set
        {
            SetProperty(ref _selectedRepeatActOption, value);
            int index = RepeatActOptions.IndexOf(value);
            int maxAct = RepeatActOptions.Count - 1;
            _targetService.RepeatAct(index, maxAct);
            if (value == null) return;
            _currentlyRepeatingAct = value;
        }
    }

    private bool _isRepeatActEnabled;

    public bool IsRepeatActEnabled
    {
        get => _isRepeatActEnabled;
        set
        {
            if (!SetProperty(ref _isRepeatActEnabled, value)) return;
            if (_isRepeatActEnabled)
            {
                for (int i = RepeatActOptions.Count - 1; i >= 0; i--)
                {
                    if (RepeatActOptions[i] != "None")
                        RepeatActOptions.RemoveAt(i);
                }

                int[] acts = _targetService.GetActs();
                foreach (int act in acts)
                {
                    string actLabel = $"Act {act}";
                    if (!RepeatActOptions.Contains(actLabel))
                        RepeatActOptions.Add(actLabel);
                }

                if (!IsCurrentTargetRepeating())
                {
                    _targetService.DisableRepeatAct();
                    _currentlyRepeatingAct = "None";
                }

                if (!RepeatActOptions.Contains(_currentlyRepeatingAct))
                {
                    _currentlyRepeatingAct = "None";
                }

                SelectedRepeatActOption = _currentlyRepeatingAct;
            }
            else
            {
                if (!IsCurrentTargetRepeating()) return;
                _targetService.DisableRepeatAct();
                _currentlyRepeatingAct = "None";
            }
        }
    }

    private bool _isResistancesWindowOpen;

    public bool IsResistancesWindowOpen
    {
        get => _isResistancesWindowOpen;
        set
        {
            if (!SetProperty(ref _isResistancesWindowOpen, value)) return;
            if (value)
                OpenResistancesWindow();
            else
                CloseResistancesWindow();
        }
    }

    public bool ShowBleedAndNotImmune => ShowBleed && !IsBleedImmune;
    public bool ShowPoisonAndNotImmune => ShowPoison && !IsPoisonImmune;
    public bool ShowToxicAndNotImmune => ShowToxic && !IsToxicImmune;

    private bool _showAllResistances;

    public bool ShowAllResistances
    {
        get => _showAllResistances;
        set
        {
            if (SetProperty(ref _showAllResistances, value))
            {
                UpdateResistancesDisplay();
            }
        }
    }

    #endregion

    #region Public Methods

    #endregion

    #region Private Methods

    private void OnLoaded()
    {
        if (IsTargetOptionsEnabled)
        {
            _targetService.ToggleTargetHook(true);
            _gameTickService.Subscribe(TargetTick);
        }

        // _enemyService.DisableRepeatAct();
        // _currentlyRepeatingAct = "None";
        AreOptionsEnabled = false;
    }

    private void OnNotLoaded()
    {
        throw new NotImplementedException();
    }

    private void RegisterHotkeys()
    {
        _hotkeyManager.RegisterAction("EnableTargetOptions",
            () => { IsTargetOptionsEnabled = !IsTargetOptionsEnabled; });
        _hotkeyManager.RegisterAction("ShowAllResistances", () =>
        {
            _showAllResistances = !_showAllResistances;
            UpdateResistancesDisplay();
        });
        _hotkeyManager.RegisterAction("FreezeHp", () => { IsFreezeHealthEnabled = !IsFreezeHealthEnabled; });
        _hotkeyManager.RegisterAction("DisableTargetAi",
            () => { IsDisableTargetAiEnabled = !IsDisableTargetAiEnabled; });
        _hotkeyManager.RegisterAction("IncreaseTargetSpeed", () => SetSpeed(Math.Min(5, TargetSpeed + 0.25f)));
        _hotkeyManager.RegisterAction("DecreaseTargetSpeed", () => SetSpeed(Math.Max(0, TargetSpeed - 0.25f)));
    }

    private void TargetTick()
    {
        if (!IsTargetValid())
        {
            IsValidTarget = false;
            return;
        }

        IsValidTarget = true;
        CurrentHealth = _targetService.GetHp();
        MaxHealth = _targetService.GetMaxHp();

        nint chrIns = _targetService.GetChrIns();

        if (chrIns != _currentChrIns)
        {
            
#if DEBUG

            // uint entityId = _targetService.GetEntityId();
            // int npcThinkParamId = _targetService.GetNpcThinkParamId();
            // int chrId = _targetService.GetNpcChrId();
            // uint npcParamId = _targetService.GetNpcParamId();
            // Console.WriteLine(
            //     $@"EntityId: {entityId} NpcThinkParamId: {npcThinkParamId} NpcParamId: {npcParamId} ChrId: {chrId}");
            // var aiThink = _targetService.GetAiThinkPtr();
            // Console.WriteLine($@"Locked on target chrIns: 0x{(long)chrIns:X} AiThink: 0x{(long)aiThink:X}");
            Console.WriteLine($@"Locked on target chrIns: 0x{(long)chrIns:X}");

#endif
            
            IsDisableTargetAiEnabled = _targetService.IsAiDisabled();
            IsRepeatActEnabled = IsCurrentTargetRepeating();
            IsFreezeHealthEnabled = _targetService.IsNoDamageEnabled();
            _currentChrIns = chrIns;
            TargetMaxPoise = _targetService.GetMaxPoise();
            SetResistances();
            TargetMaxBleed = IsBleedImmune ? 0 : _targetService.GetMaxBleed();
            TargetMaxPoison = IsPoisonImmune ? 0 : _targetService.GetMaxPoison();
            TargetMaxToxic = IsToxicImmune ? 0 : _targetService.GetMaxToxic();
            if (!IsResistancesWindowOpen || _resistancesWindowWindow == null) return;
            _resistancesWindowWindow.DataContext = null;
            _resistancesWindowWindow.DataContext = this;
        }

        TargetSpeed = _targetService.GetSpeed();
        TargetCurrentPoise = _targetService.GetPoise();
        TargetPoiseTimer = _targetService.GetPoiseTimer();
        TargetCurrentBleed = IsBleedImmune ? 0 : _targetService.GetCurrentBleed();
        TargetCurrentPoison = IsPoisonImmune ? 0 : _targetService.GetCurrentPoison();
        TargetCurrentToxic = IsToxicImmune ? 0 : _targetService.GetCurrentToxic();
    }

    private bool IsTargetValid()
    {
        nint targetId = _targetService.GetChrIns();
        if (targetId == 0)
            return false;

        float health = _targetService.GetHp();
        float maxHealth = _targetService.GetMaxHp();
        if (health < 0 || maxHealth <= 0 || health > 10000000 || maxHealth > 10000000)
            return false;

        if (health > maxHealth * 1.5) return false;

        var position = _targetService.GetPosition();

        if (float.IsNaN(position.X) || float.IsNaN(position.Y) || float.IsNaN(position.Z))
            return false;

        if (Math.Abs(position.X) > 10000 || Math.Abs(position.Y) > 10000 || Math.Abs(position.Z) > 10000)
            return false;

        return true;
    }

    private void UpdateResistancesDisplay()
    {
        if (!IsTargetOptionsEnabled) return;
        if (_showAllResistances)
        {
            ShowBleed = true;
            ShowPoise = true;
            ShowPoison = true;
            ShowToxic = true;
        }
        else
        {
            ShowBleed = false;
            ShowPoise = false;
            ShowPoison = false;
            ShowToxic = false;
        }

        if (!IsResistancesWindowOpen || _resistancesWindowWindow == null) return;
        _resistancesWindowWindow.DataContext = null;
        _resistancesWindowWindow.DataContext = this;
    }

    private void SetResistances()
    {
        IsToxicImmune = false;
        IsPoisonImmune = false;
        IsBleedImmune = false;
        int immunity = _enemyService.GetImmunitySpEffect();
        switch (immunity)
        {
            case 90000:
            case 90001:
            {
                IsToxicImmune = true;
                IsPoisonImmune = true;
                IsBleedImmune = true;
                break;
            }
            case 90010:
            case 90101:
            {
                IsPoisonImmune = true;
                IsBleedImmune = true;
                break;
            }
            case 90011:
            case 90100:
            {
                IsToxicImmune = true;
                IsPoisonImmune = true;
                break;
            }
            case 91001:
            case 91000:
            {
                IsBleedImmune = true;
                IsToxicImmune = true;
                break;
            }

            case 91011:
            case 91010:
            {
                IsBleedImmune = true;
                break;
            }
            case 91100:
            case 91101:
            {
                IsToxicImmune = true;
                break;
            }
            case 90111:
            case 90110:
            {
                IsPoisonImmune = true;
                break;
            }
        }
    }
    
    public void SetTargetHealth(int value)
    {
        int health = MaxHealth * value / 100;
        _targetService.SetHp(health);
    }

    public void SetSpeed(float value)
    {
        TargetSpeed = value;
    }
    
    private void OpenResistancesWindow()
    {
        if (_resistancesWindowWindow != null && _resistancesWindowWindow.IsVisible) return;
        _resistancesWindowWindow = new ResistancesWindow
        {
            DataContext = this
        };
        _resistancesWindowWindow.Closed += (s, e) => _isResistancesWindowOpen = false;
        _resistancesWindowWindow.Show();
    }

    private void CloseResistancesWindow()
    {
        if (_resistancesWindowWindow == null || !_resistancesWindowWindow.IsVisible) return;
        _resistancesWindowWindow.Close();
        _resistancesWindowWindow = null;
    }
    
    private bool IsCurrentTargetRepeating()
    {
        var currentRepeatEnemyId = _targetService.GetCurrentRepeatEnemyId();
        var lockedTargetId = _targetService.GetEnemyBattleId();
        return currentRepeatEnemyId == lockedTargetId;
    }

    #endregion
}