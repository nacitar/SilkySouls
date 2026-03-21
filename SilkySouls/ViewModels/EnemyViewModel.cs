using System.Windows.Threading;
using SilkySouls.Services;
using SilkySouls.Utilities;
using SilkySouls.Views;

namespace SilkySouls.ViewModels
{
    public class EnemyViewModel : BaseViewModel
    {
        private bool _areOptionsEnabled;

        private readonly DispatcherTimer _targetOptionsTimer;

        private ResistancesWindow _resistancesWindowWindow;

        private bool _isAllNoDamageEnabled;
        private bool _isAllNoDeathEnabled;
        private bool _is4KingsTimerStopped;

        private readonly EnemyService _enemyService;
        private readonly HotkeyManager _hotkeyManager;

        public EnemyViewModel(EnemyService enemyService, HotkeyManager hotkeyManager)
        {
            _enemyService = enemyService;
            _hotkeyManager = hotkeyManager;

            RegisterHotkeys();
        }

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction("DisableAi", () => { IsDisableAiEnabled = !IsDisableAiEnabled; });
            _hotkeyManager.RegisterAction("AllNoDeath", () => { IsAllNoDeathEnabled = !IsAllNoDeathEnabled; });
            _hotkeyManager.RegisterAction("AllNoDamage", () => { IsAllNoDamageEnabled = !IsAllNoDamageEnabled; });
        }

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

        public void DisableButtons()
        {
            _targetOptionsTimer.Stop();
            // IsFreezeHealthEnabled = false;
            // IsRepeatActEnabled = false;
            // _enemyService.DisableRepeatAct();
            // _currentlyRepeatingAct = "None";
            AreOptionsEnabled = false;
        }

        public void TryEnableActiveOptions()
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
    }
}