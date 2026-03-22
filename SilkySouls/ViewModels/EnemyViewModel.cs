using System.Windows.Input;
using SilkySouls.Core;
using SilkySouls.Enums;
using SilkySouls.GameIds;
using SilkySouls.Interfaces;
using SilkySouls.Services;
using SilkySouls.Utilities;

namespace SilkySouls.ViewModels
{
    public class EnemyViewModel : BaseViewModel
    {
        private readonly EnemyService _enemyService;
        private readonly HotkeyManager _hotkeyManager;
        private readonly IEmevdService _emevdService;

        public EnemyViewModel(EnemyService enemyService, HotkeyManager hotkeyManager, IStateService stateService,
            IEmevdService emevdService)
        {
            _enemyService = enemyService;
            _hotkeyManager = hotkeyManager;
            _emevdService = emevdService;

            stateService.Subscribe(State.Loaded, OnLoaded);
            stateService.Subscribe(State.NotLoaded, OnNotLoaded);

            TestCommand = new DelegateCommand(Test);

            RegisterHotkeys();
        }

        #region Commands

        public ICommand TestCommand { get; }

        #endregion

        #region Properties

        private bool _areOptionsEnabled;

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }

        private bool _isDisableAiEnabled;

        public bool IsDisableAiEnabled
        {
            get => _isDisableAiEnabled;
            set
            {
                if (SetProperty(ref _isDisableAiEnabled, value))
                {
                    _enemyService.ToggleAi(_isDisableAiEnabled ? 1 : 0);
                }
            }
        }

        private bool _isAllNoDamageEnabled;

        public bool IsAllNoDamageEnabled
        {
            get => _isAllNoDamageEnabled;
            set
            {
                if (SetProperty(ref _isAllNoDamageEnabled, value))
                {
                    _enemyService.ToggleAllNoDamage(_isAllNoDamageEnabled ? 1 : 0);
                }
            }
        }

        private bool _isAllNoDeathEnabled;

        public bool IsAllNoDeathEnabled
        {
            get => _isAllNoDeathEnabled;
            set
            {
                if (SetProperty(ref _isAllNoDeathEnabled, value))
                {
                    _enemyService.ToggleAllNoDeath(_isAllNoDeathEnabled ? 1 : 0);
                }
            }
        }

        private bool _is4KingsTimerStopped;

        public bool Is4KingsTimerStopped
        {
            get => _is4KingsTimerStopped;
            set
            {
                if (SetProperty(ref _is4KingsTimerStopped, value))
                {
                    _enemyService.Toggle4KingsTimer(_is4KingsTimerStopped);
                }
            }
        }

        #endregion

        #region Private Methods

        private void Test()
        {
            _emevdService.ExecuteEmevdCommand(Emevd.EmevdCommands.ForceCharacterDeath);
        }

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction(HotkeyActions.DisableAi, () => { IsDisableAiEnabled = !IsDisableAiEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.AllNoDeath, () => { IsAllNoDeathEnabled = !IsAllNoDeathEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.AllNoDamage, () => { IsAllNoDamageEnabled = !IsAllNoDamageEnabled; });
        }

        private void OnNotLoaded()
        {
            AreOptionsEnabled = false;
        }

        private void OnLoaded()
        {
            if (IsDisableAiEnabled)
                _enemyService.ToggleAi(1);
            if (IsAllNoDamageEnabled)
                _enemyService.ToggleAllNoDamage(1);
            if (IsAllNoDeathEnabled)
                _enemyService.ToggleAllNoDeath(1);
            if (Is4KingsTimerStopped)
                _enemyService.Toggle4KingsTimer(true);
            AreOptionsEnabled = true;
        }

        #endregion
    }
}
