using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using H.Hooks;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Utilities;

namespace SilkySouls.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly HotkeyManager _hotkeyManager;
        private readonly ISettingsService _settingsService;
        private readonly Dictionary<string, HotkeyBindingViewModel> _hotkeyLookup;

        private string _currentSettingHotkeyId;
        private LowLevelKeyboardHook _tempHook;
        private Keys _currentKeys;
        private bool _isLoaded;

        public Dictionary<string, List<HotkeyBindingViewModel>> Hotkeys { get; }

        public SettingsViewModel(ISettingsService settingsService, HotkeyManager hotkeyManager,
            IStateService stateService)
        {
            _settingsService = settingsService;
            _hotkeyManager = hotkeyManager;
            
            stateService.Subscribe(State.Loaded, OnLoaded);

            Hotkeys = new Dictionary<string, List<HotkeyBindingViewModel>>
            {
                ["Player"] =
                [
                    new("Save Position 1", HotkeyActions.SavePos1),
                    new("Save Position 2", HotkeyActions.SavePos2),
                    new("Restore Position 1", HotkeyActions.RestorePos1),
                    new("Restore Position 2", HotkeyActions.RestorePos2),
                    new("RTSR Setup", HotkeyActions.RTSR),
                    new("No Death", HotkeyActions.NoDeath),
                    new("One Shot", HotkeyActions.OneShot),
                    new("Restore Spells", HotkeyActions.RestoreSpellCasts),
                    new("Toggle Speed", HotkeyActions.ToggleSpeed),
                    new("Increase Speed", HotkeyActions.IncreaseSpeed),
                    new("Decrease Speed", HotkeyActions.DecreaseSpeed),
                ],
                ["Enemies"] =
                [
                    new("Disable All AI", HotkeyActions.DisableAi),
                    new("All No Death", HotkeyActions.AllNoDeath),
                    new("All No Damage", HotkeyActions.AllNoDamage),
                ],
                ["Target"] =
                [
                    new("Enable Target Options", HotkeyActions.EnableTargetOptions),
                    new("Show All Resistances", HotkeyActions.ShowAllResistances),
                    new("Freeze HP", HotkeyActions.FreezeHp),
                    new("Disable Target AI", HotkeyActions.DisableTargetAi),
                    new("Increase Target Speed", HotkeyActions.IncreaseTargetSpeed),
                    new("Decrease Target Speed", HotkeyActions.DecreaseTargetSpeed),
                ],
                ["Utility"] =
                [
                    new("Quitout", HotkeyActions.Quitout),
                    new("No Clip", HotkeyActions.NoClip),
                    new("Warp", HotkeyActions.Warp),
                ],
            };

            _hotkeyLookup = Hotkeys.Values
                .SelectMany(x => x)
                .ToDictionary(h => h.ActionId);

            LoadHotkeyDisplays();
            RegisterHotkeys();
        }

        #region Properties

        private bool _isFastQuitoutEnabled;

        public bool IsFastQuitoutEnabled
        {
            get => _isFastQuitoutEnabled;
            set
            {
                if (SetProperty(ref _isFastQuitoutEnabled, value))
                {
                    SettingsManager.Default.FastQuitout = value;
                    SettingsManager.Default.Save();
                    if (_isLoaded)
                    {
                        _settingsService.ToggleFastQuitout(_isFastQuitoutEnabled ? 1 : 0);
                    }
                }
            }
        }

        private bool _isAlwaysOnTopEnabled;

        public bool IsAlwaysOnTopEnabled
        {
            get => _isAlwaysOnTopEnabled;
            set
            {
                if (!SetProperty(ref _isAlwaysOnTopEnabled, value)) return;
                SettingsManager.Default.AlwaysOnTop = value;
                SettingsManager.Default.Save();
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow != null) mainWindow.Topmost = _isAlwaysOnTopEnabled;
            }
        }

        private bool _isEnableHotkeysEnabled;

        public bool IsEnableHotkeysEnabled
        {
            get => _isEnableHotkeysEnabled;
            set
            {
                if (SetProperty(ref _isEnableHotkeysEnabled, value))
                {
                    SettingsManager.Default.EnableHotkeys = value;
                    SettingsManager.Default.Save();
                    if (_isEnableHotkeysEnabled) _hotkeyManager.Start();
                    else _hotkeyManager.Stop();
                }
            }
        }

        #endregion

        #region Public Methods

        public void StartSettingHotkey(string actionId)
        {
            if (_currentSettingHotkeyId != null &&
                _hotkeyLookup.TryGetValue(_currentSettingHotkeyId, out var prev))
            {
                prev.HotkeyText = GetHotkeyDisplayText(_currentSettingHotkeyId);
            }

            _currentSettingHotkeyId = actionId;

            if (_hotkeyLookup.TryGetValue(actionId, out var current))
            {
                current.HotkeyText = "Press keys...";
            }

            _tempHook = new LowLevelKeyboardHook();
            _tempHook.IsExtendedMode = true;
            _tempHook.Down += TempHook_Down;
            _tempHook.Start();
        }

        public void ConfirmHotkey()
        {
            var currentSettingHotkeyId = _currentSettingHotkeyId;
            var currentKeys = _currentKeys;
            if (currentSettingHotkeyId == null || currentKeys == null || currentKeys.IsEmpty)
            {
                CancelSettingHotkey();
                return;
            }

            HandleExistingHotkey(currentKeys);
            SetNewHotkey(currentSettingHotkeyId, currentKeys);

            StopSettingHotkey();
        }

        public void CancelSettingHotkey()
        {
            var actionId = _currentSettingHotkeyId;

            if (actionId != null && _hotkeyLookup.TryGetValue(actionId, out var binding))
            {
                binding.HotkeyText = "None";
                _hotkeyManager.SetHotkey(actionId, new Keys());
            }

            StopSettingHotkey();
        }

        
        public void ApplyStartUpOptions()
        {
            _isEnableHotkeysEnabled = SettingsManager.Default.EnableHotkeys;
            if (_isEnableHotkeysEnabled) _hotkeyManager.Start();
            else _hotkeyManager.Stop();
            OnPropertyChanged(nameof(IsEnableHotkeysEnabled));
            _isFastQuitoutEnabled = SettingsManager.Default.FastQuitout;
            OnPropertyChanged(nameof(IsFastQuitoutEnabled));
            IsAlwaysOnTopEnabled = SettingsManager.Default.AlwaysOnTop;
        }
        
        #endregion

        #region Private Methods

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction(HotkeyActions.Quitout, () => _settingsService.Quitout());
        }
        
        private void OnLoaded()
        {
            if (IsFastQuitoutEnabled) _settingsService.ToggleFastQuitout(1);
        }

        private void LoadHotkeyDisplays()
        {
            foreach (var hotkey in _hotkeyLookup.Values)
            {
                hotkey.HotkeyText = GetHotkeyDisplayText(hotkey.ActionId);
            }
        }

        private string GetHotkeyDisplayText(string actionId)
        {
            Keys keys = _hotkeyManager.GetHotkey(actionId);
            return keys != null && keys.Values.ToArray().Length > 0 ? string.Join(" + ", keys) : "None";
        }

        private void TempHook_Down(object sender, KeyboardEventArgs e)
        {
            if (_currentSettingHotkeyId == null || e.Keys.IsEmpty)
                return;

            try
            {
                bool containsEnter = e.Keys.Values.Contains(Key.Enter) || e.Keys.Values.Contains(Key.Return);

                if (containsEnter && _currentKeys != null)
                {
                    _hotkeyManager.SetHotkey(_currentSettingHotkeyId, _currentKeys);
                    StopSettingHotkey();
                    e.IsHandled = true;
                    return;
                }

                if (e.Keys.Values.Contains(Key.Escape))
                {
                    CancelSettingHotkey();
                    e.IsHandled = true;
                    return;
                }

                if (containsEnter)
                {
                    e.IsHandled = true;
                    return;
                }

                if (e.Keys.IsEmpty)
                    return;

                _currentKeys = e.Keys;

                if (_hotkeyLookup.TryGetValue(_currentSettingHotkeyId, out var binding))
                {
                    binding.HotkeyText = e.Keys.ToString();
                }
            }
            catch (Exception ex)
            {
                if (_hotkeyLookup.TryGetValue(_currentSettingHotkeyId, out var binding))
                {
                    binding.HotkeyText = "Error: Invalid key combination";
                }
            }

            e.IsHandled = true;
        }

        private void StopSettingHotkey()
        {
            if (_tempHook != null)
            {
                _tempHook.Down -= TempHook_Down;
                _tempHook.Dispose();
                _tempHook = null;
            }

            _currentSettingHotkeyId = null;
            _currentKeys = null;
        }

        private void HandleExistingHotkey(Keys currentKeys)
        {
            string existingHotkeyId = _hotkeyManager.GetActionIdByKeys(currentKeys);
            if (string.IsNullOrEmpty(existingHotkeyId)) return;

            _hotkeyManager.ClearHotkey(existingHotkeyId);
            if (_hotkeyLookup.TryGetValue(existingHotkeyId, out var binding))
            {
                binding.HotkeyText = "None";
            }
        }

        private void SetNewHotkey(string currentSettingHotkeyId, Keys currentKeys)
        {
            _hotkeyManager.SetHotkey(currentSettingHotkeyId, currentKeys);

            if (_hotkeyLookup.TryGetValue(currentSettingHotkeyId, out var binding))
            {
                binding.HotkeyText = new Keys(currentKeys.Values.ToArray()).ToString();
            }
        }

        #endregion
    }
}
