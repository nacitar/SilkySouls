using System.Threading.Tasks;
using System.Windows.Input;
using SilkySouls.Core;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Services;
using SilkySouls.Utilities;
using static SilkySouls.GameIds.EzState;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.ViewModels
{
    public class UtilityViewModel : BaseViewModel
    {
        private bool _wasNoDeathEnabled;
        private bool _wasNoDmgEnabled;

        private bool _areAttachedOptionsRestored;

        private readonly UtilityService _utilityService;
        private readonly PlayerViewModel _playerViewModel;
        private readonly IParamService _paramService;
        private readonly IEzStateService _ezStateService;
        private readonly HotkeyManager _hotkeyManager;

        public const int EquipParamGoodsTableIdx = 3;
        public const int EstusParamRowIdx = 32;
        public const int LordVesselIconId = 2085;
        public const int IconIdOffset = 0x2C;

        public UtilityViewModel(UtilityService utilityService, HotkeyManager hotkeyManager,
            PlayerViewModel playerViewModel, IParamService paramService, IStateService stateService,
            IEzStateService ezStateService)
        {
            _utilityService = utilityService;
            _playerViewModel = playerViewModel;
            _paramService = paramService;
            _ezStateService = ezStateService;
            _hotkeyManager = hotkeyManager;

            stateService.Subscribe(State.Loaded, OnLoaded);
            stateService.Subscribe(State.NotLoaded, OnNotLoaded);

            ShowLevelUpMenuCommand = new DelegateCommand(ShowLevelUpMenu);
            ShowAttunementMenuCommand = new DelegateCommand(ShowAttunementMenu);
            UpgradeWeaponCommand = new DelegateCommand(UpgradeWeapon);
            UpgradeArmorCommand = new DelegateCommand(UpgradeArmor);
            OpenFeedMenuCommand = new DelegateCommand(OpenFeedMenu);
            OpenWarpMenuCommand = new DelegateCommand(OpenWarpMenu);
            OpenBottomlessBoxCommand = new DelegateCommand(OpenBottomlessBox);
            OpenShopCommand = new DelegateCommand<int[]>(OpenShop);

            RegisterHotkeys();
        }

        #region Commands

        public ICommand ShowLevelUpMenuCommand { get; }
        public ICommand ShowAttunementMenuCommand { get; }
        public ICommand UpgradeWeaponCommand { get; }
        public ICommand UpgradeArmorCommand { get; }
        public ICommand OpenFeedMenuCommand { get; }
        public ICommand OpenWarpMenuCommand { get; }
        public ICommand OpenBottomlessBoxCommand { get; }
        public ICommand OpenShopCommand { get; }

        #endregion

        #region Properties

        private bool _areOptionsEnabled;

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }

        private bool _areAttachedOptionsEnabled;

        public bool AreAttachedOptionsEnabled
        {
            get => _areAttachedOptionsEnabled;
            set => SetProperty(ref _areAttachedOptionsEnabled, value);
        }

        private bool _isDrawEnabled;

        public bool IsDrawEnabled
        {
            get => _isDrawEnabled;
            set
            {
                if (!SetProperty(ref _isDrawEnabled, value)) return;
                if (value)
                {
                    _utilityService.EnableDraw();
                }
                else
                {
                    _utilityService.DisableDraw();
                    IsHitboxEnabled = false;
                    IsDrawEventEnabled = false;
                    IsSoundViewEnabled = false;
                }
            }
        }

        private bool _isHitboxEnabled;

        public bool IsHitboxEnabled
        {
            get => _isHitboxEnabled;
            set
            {
                if (!SetProperty(ref _isHitboxEnabled, value)) return;
                if (_isHitboxEnabled)
                {
                    _utilityService.EnableHitboxView();
                }
                else
                {
                    _utilityService.DisableHitboxView();
                }
            }
        }

        private bool _isSoundViewEnabled;

        public bool IsSoundViewEnabled
        {
            get => _isSoundViewEnabled;
            set
            {
                if (!SetProperty(ref _isSoundViewEnabled, value)) return;
                if (_isSoundViewEnabled)
                {
                    _utilityService.EnableSoundView();
                }
                else
                {
                    _utilityService.DisableSoundView();
                }
            }
        }

        private bool _isDrawEventEnabled;

        public bool IsDrawEventEnabled
        {
            get => _isDrawEventEnabled;
            set
            {
                if (!SetProperty(ref _isDrawEventEnabled, value)) return;
                if (_isDrawEventEnabled)
                {
                    _utilityService.EnableDrawEvent();
                }
                else
                {
                    _utilityService.DisableDrawEvent();
                }
            }
        }

        private bool _isNoClipEnabled;

        public bool IsNoClipEnabled
        {
            get => _isNoClipEnabled;
            set
            {
                if (!SetProperty(ref _isNoClipEnabled, value)) return;
                if (_isNoClipEnabled)
                {
                    _utilityService.EnableNoClip();
                    _wasNoDeathEnabled = _playerViewModel.IsNoDeathEnabled;
                    _wasNoDmgEnabled = _playerViewModel.IsNoDamageEnabled;
                    _playerViewModel.IsNoDeathEnabled = true;
                    _playerViewModel.IsNoDamageEnabled = true;
                    _playerViewModel.IsSilentEnabled = true;
                    _playerViewModel.IsInvisibleEnabled = true;
                }
                else
                {
                    _utilityService.DisableNoClip();
                    _playerViewModel.IsNoDeathEnabled = _wasNoDeathEnabled;
                    _playerViewModel.IsNoDamageEnabled = _wasNoDmgEnabled;
                    _playerViewModel.IsSilentEnabled = false;
                    _playerViewModel.IsInvisibleEnabled = false;
                }
            }
        }

        private bool _isDeathCamEnabled;

        public bool IsDeathCamEnabled
        {
            get => _isDeathCamEnabled;
            set
            {
                if (!SetProperty(ref _isDeathCamEnabled, value)) return;
                _utilityService.ToggleDeathCam(_isDeathCamEnabled);
            }
        }

        private bool _isFilterRemoveEnabled;

        public bool IsFilterRemoveEnabled
        {
            get => _isFilterRemoveEnabled;
            set
            {
                if (!SetProperty(ref _isFilterRemoveEnabled, value)) return;
                _utilityService.ToggleFilter(_isFilterRemoveEnabled);
            }
        }

        private bool _isGuaranteedBkhEnabled;

        public bool IsGuaranteedBkhEnabled
        {
            get => _isGuaranteedBkhEnabled;
            set
            {
                if (SetProperty(ref _isGuaranteedBkhEnabled, value))
                {
                    if (AreOptionsEnabled)
                    {
                        var estusRow = _paramService.GetParamRow(EquipParamGoodsTableIdx, 0, EstusParamRowIdx);
                        _paramService.WriteInt32(estusRow, IconIdOffset, LordVesselIconId);
                        _utilityService.SetGuaranteedBkhDrop(_isGuaranteedBkhEnabled);
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        public void ResetAttached()
        {
            IsNoClipEnabled = false;
            _areAttachedOptionsRestored = false;
        }

        public void TryRestoreAttachedFeatures()
        {
            if (_areAttachedOptionsRestored) return;
            if (IsDrawEnabled)
            {
                if (!_utilityService.EnableDraw()) return;
            }

            _areAttachedOptionsRestored = true;
        }

        public void DisableNoClip()
        {
            _utilityService.DisableNoClip();
            IsNoClipEnabled = false;
        }

        #endregion

        #region Private Methods

        private void ShowLevelUpMenu() => _utilityService.ShowMenu(MenuMan.MenuManData.LevelUpMenu);
        private void ShowAttunementMenu() => _ezStateService.ExecuteTalkCommand(TalkCommands.OpenAttunement);
        private void UpgradeWeapon() => _ezStateService.ExecuteTalkCommand(TalkCommands.OpenEnhanceWeapon);
        private void UpgradeArmor() => _ezStateService.ExecuteTalkCommand(TalkCommands.OpenEnhanceArmor);
        private void OpenFeedMenu() => _utilityService.ShowMenu(MenuMan.MenuManData.Feed);
        private void OpenWarpMenu() => _utilityService.ShowMenu(MenuMan.MenuManData.Warp);
        private void OpenBottomlessBox() => _utilityService.ShowMenu(MenuMan.MenuManData.BottomlessBox);

        private void OpenShop(int[] shopParams) =>
            _ezStateService.ExecuteTalkCommand(TalkCommands.OpenRegularShop(shopParams[0], shopParams[1]));

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction(HotkeyActions.NoClip, () => { IsNoClipEnabled = !IsNoClipEnabled; });
        }

        private void OnNotLoaded()
        {
            IsNoClipEnabled = false;
            AreOptionsEnabled = false;
        }

        private void OnLoaded()
        {
            if (IsHitboxEnabled)
                _utilityService.EnableHitboxView();
            if (IsSoundViewEnabled)
                _utilityService.EnableSoundView();
            if (IsDrawEventEnabled)
                _utilityService.EnableDrawEvent();
            if (IsFilterRemoveEnabled)
                _utilityService.ToggleFilter(IsFilterRemoveEnabled);
            if (IsDeathCamEnabled)
                _utilityService.ToggleDeathCam(IsDeathCamEnabled);
            if (IsGuaranteedBkhEnabled)
            {
                _utilityService.SetGuaranteedBkhDrop(true);
                _ = Task.Run(() =>
                {
                    Task.Delay(500).Wait();
                    var estusRow = _paramService.GetParamRow(EquipParamGoodsTableIdx, 0, EstusParamRowIdx);
                    _paramService.WriteInt32(estusRow, IconIdOffset, LordVesselIconId);
                });
            }
            else
            {
                _utilityService.SetGuaranteedBkhDrop(false);
            }

            AreOptionsEnabled = true;
        }

        #endregion
    }
}