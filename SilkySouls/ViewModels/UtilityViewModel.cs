using System.Threading.Tasks;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Services;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.ViewModels
{
    public class UtilityViewModel : BaseViewModel
    {
        private bool _isDrawEnabled;
        private bool _isHitboxEnabled;
        private bool _isSoundViewEnabled;
        private bool _isDrawEventEnabled;
        private bool _isTargetingViewEnabled;
        private bool _isNoClipEnabled;
        private bool _isDeathCamEnabled;
        private bool _isFilterRemoveEnabled;

        private bool _areButtonsEnabled;
        private bool _areAttachedOptionsEnabled;
        private bool _areAttachedOptionsRestored;

        private bool _wasNoDeathEnabled;
        private bool _wasNoDmgEnabled;

        private readonly UtilityService _utilityService;
        private readonly PlayerViewModel _playerViewModel;
        private readonly IParamService _paramService;
        private readonly HotkeyManager _hotkeyManager;

        public const int EquipParamGoodsTableIdx = 3;
        public const int EstusParamRowIdx = 32;
        public const int LordVesselIconId = 2085;
        public const int IconIdOffset = 0x2C;

        public UtilityViewModel(UtilityService utilityService, HotkeyManager hotkeyManager,
            PlayerViewModel playerViewModel, IParamService paramService, IStateService stateService)
        {
            _utilityService = utilityService;
            _playerViewModel = playerViewModel;
            _paramService = paramService;
            _hotkeyManager = hotkeyManager;

            stateService.Subscribe(State.Loaded, OnLoaded);
            stateService.Subscribe(State.NotLoaded, OnNotLoaded);

            RegisterHotkeys();
        }

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction(HotkeyActions.NoClip, () => { IsNoClipEnabled = !IsNoClipEnabled; });
            
        }
        
        public bool AreButtonsEnabled
        {
            get => _areButtonsEnabled;
            set => SetProperty(ref _areButtonsEnabled, value);
        }

        public bool AreAttachedOptionsEnabled
        {
            get => _areAttachedOptionsEnabled;
            set => SetProperty(ref _areAttachedOptionsEnabled, value);
        }


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

        public bool IsDeathCamEnabled
        {
            get => _isDeathCamEnabled;
            set
            {
                if (!SetProperty(ref _isDeathCamEnabled, value)) return;
                _utilityService.ToggleDeathCam(_isDeathCamEnabled);
            }
        }
        
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
                    if (AreButtonsEnabled)
                    {
                       var estusRow = _paramService.GetParamRow(EquipParamGoodsTableIdx, 0, EstusParamRowIdx);
                        _paramService.WriteInt32(estusRow, IconIdOffset, LordVesselIconId);
                        _utilityService.SetGuaranteedBkhDrop(_isGuaranteedBkhEnabled);
                    }
                }
            }
        }
        
        private void OnNotLoaded()
        {
            IsNoClipEnabled = false;
            AreButtonsEnabled = false;
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
            AreButtonsEnabled = true;
        }

        public void ResetAttached()
        {
            IsNoClipEnabled = false;
            _areAttachedOptionsRestored = false;
            _utilityService.ResetBools();
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

        public void ShowLevelUpMenu() => _utilityService.ShowMenu(MenuMan.MenuManData.LevelUpMenu);
        
        public void ShowAttunementMenu() => _utilityService.OpenAttunement();
        

        public void ShowUpgradeMenu(bool isWeapon) => _utilityService.ShowUpgradeMenu(isWeapon);

       
        public void OpenShop(ulong[] shopParams) => _utilityService.OpenRegularShop(shopParams);
        
        public void OpenFeedMenu()=> _utilityService.ShowMenu(MenuMan.MenuManData.Feed);
        
        public void OpenWarpMenu()=> _utilityService.ShowMenu(MenuMan.MenuManData.Warp);

        public void OpenBottomlessBox() => _utilityService.ShowMenu(MenuMan.MenuManData.BottomlessBox);

        public void DisableNoClip()
        {
            _utilityService.DisableNoClip();
            IsNoClipEnabled = false;
        }
    }
}