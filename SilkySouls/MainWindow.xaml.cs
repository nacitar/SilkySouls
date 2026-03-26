using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.memory;
using SilkySouls.Memory;
using SilkySouls.Services;
using SilkySouls.Utilities;
using SilkySouls.ViewModels;
using SilkySouls.Views;

namespace SilkySouls
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly IMemoryService _memoryService;
        private readonly DispatcherTimer _gameLoadedTimer;

        private readonly PlayerViewModel _playerViewModel;
        private readonly UtilityViewModel _utilityViewModel;
        private readonly ItemViewModel _itemViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly HookManager _hookManager;
        private readonly AoBScanner _aobScanner;
        private readonly ItemService _itemService;
        private readonly IStateService _stateService;

        public MainWindow()
        {
            _memoryService = new MemoryService();
            _memoryService.StartAutoAttach();
            _stateService = new StateService(_memoryService);

            InitializeComponent();
            if (SettingsManager.Default.WindowLeft != 0 || SettingsManager.Default.WindowTop != 0)
            {
                Left = SettingsManager.Default.WindowLeft;
                Top = SettingsManager.Default.WindowTop;
            }
            else WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var hotkeyManager = new HotkeyManager(_memoryService);
            _hookManager = new HookManager(_memoryService);

            IGameTickService gameTickService = new GameTickService(_stateService);

            ITravelService travelService = new TravelService(_memoryService, _hookManager);
            IPlayerService playerService = new PlayerService(_memoryService, travelService);

            ITargetService targetService = new TargetService(_memoryService, _hookManager);

            _aobScanner = new AoBScanner(_memoryService);

            IEmevdService emevdService = new EmevdService(_memoryService);
            IEzStateService ezStateService = new EzStateService(_memoryService);
            IEnemyService enemyService = new EnemyService(_memoryService, _hookManager);
            IEventService eventService = new EventService(_memoryService, playerService, emevdService);
            IUtilityService utilityService = new UtilityService(_memoryService, _hookManager);
            IDebugDrawService debugDrawService = new DebugDrawService(_memoryService, _hookManager, _stateService);
            var utilityServiceOld = new UtilityServiceOld(_memoryService, _hookManager);
            IParamService paramService = new ParamService(_memoryService);
            _itemService = new ItemService(_memoryService);
            var settingsService = new SettingsService(_memoryService);


            _playerViewModel = new PlayerViewModel(playerService, hotkeyManager, _stateService, gameTickService);
            TargetViewModel targetViewModel =
                new TargetViewModel(targetService, hotkeyManager, gameTickService, _stateService);
            _utilityViewModel = new UtilityViewModel(utilityServiceOld, utilityService, hotkeyManager, _playerViewModel, paramService,
                _stateService, ezStateService, debugDrawService);
            var travelViewModel = new TravelViewModel(travelService, hotkeyManager, _utilityViewModel, _stateService);
            var eventViewModel = new EventViewModel(eventService,  _stateService);
            var enemyViewModel = new EnemyViewModel(enemyService, hotkeyManager, _stateService, emevdService);
            _itemViewModel = new ItemViewModel(_itemService, _stateService);
            _settingsViewModel = new SettingsViewModel(settingsService, hotkeyManager);

            var playerTab = new PlayerTab(_playerViewModel);
            var travelTab = new TravelTab(travelViewModel);
            var eventTab = new EventTab(eventViewModel);
            var utilityTab = new UtilityTab(_utilityViewModel);
            var enemyTab = new EnemyTab(enemyViewModel);
            var targetTab = new TargetTab(targetViewModel);
            var itemTab = new ItemTab(_itemViewModel);
            var settingsTab = new SettingsTab(_settingsViewModel);

            MainTabControl.Items.Add(new TabItem { Header = "Player", Content = playerTab });
            MainTabControl.Items.Add(new TabItem { Header = "Travel", Content = travelTab });
            MainTabControl.Items.Add(new TabItem { Header = "Enemies", Content = enemyTab });
            MainTabControl.Items.Add(new TabItem { Header = "Target", Content = targetTab });
            MainTabControl.Items.Add(new TabItem { Header = "Utility", Content = utilityTab });
            MainTabControl.Items.Add(new TabItem { Header = "Event", Content = eventTab });
            MainTabControl.Items.Add(new TabItem { Header = "Items", Content = itemTab });
            MainTabControl.Items.Add(new TabItem { Header = "Settings", Content = settingsTab });

            _settingsViewModel.ApplyStartUpOptions();
            Closing += MainWindow_Closing;

            _gameLoadedTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(25)
            };
            _gameLoadedTimer.Tick += Timer_Tick;
            _gameLoadedTimer.Start();

            VersionChecker.UpdateVersionText(AppVersion);

            if (SettingsManager.Default.EnableUpdateChecks)
            {
                VersionChecker.CheckForUpdates(this);
            }
        }

        private bool _loaded;
        private bool _hasAllocatedMemory;
        private bool _hasCheckedPatch;
        private bool _hasPublishedFadedIn;
        private bool _hasPublishedLoaded;

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_memoryService.IsAttached)
            {
                IsAttachedText.Text = "Attached to game";
                IsAttachedText.Foreground = (SolidColorBrush)Application.Current.Resources["AttachedBrush"];

                if (!_hasCheckedPatch)
                {
                    if (!PatchManager.Initialize(_memoryService))
                    {
                        _aobScanner.Scan();
                    }

                    _hasCheckedPatch = true;
                }


                if (!_hasAllocatedMemory)
                {
                    _memoryService.AllocCodeCave();
#if DEBUG
                    Console.WriteLine($"Code cave: 0x{(long)CodeCaveOffsets.Base:X}");
#endif
                    _hasAllocatedMemory = true;
                }

                _utilityViewModel.TryRestoreAttachedFeatures();

                if (_stateService.IsLoaded())
                {
                    if (!_hasPublishedFadedIn && _hasPublishedLoaded && !_stateService.IsFading())
                    {
                        _stateService.Publish(State.FadedIn);
                        _hasPublishedFadedIn = true;
                    }
                    if (_loaded) return;
                    _loaded = true;
                    _hasPublishedLoaded = true;
                    _stateService.Publish(State.Loaded);
                    TrySetGameStartPrefs();
                    _settingsViewModel.ApplyLoadedOptions();
                }
                else if (_loaded)
                {
                    _stateService.Publish(State.NotLoaded);
                    _loaded = false;
                    _hasPublishedLoaded = false;
                    _hasPublishedFadedIn = false;
                }
            }
            else
            {
                _hookManager.ClearHooks();
                _stateService.Publish(State.Detached);
                _stateService.Publish(State.NotLoaded);
                _utilityViewModel.ResetAttached();
                _settingsViewModel.ResetAttached();
                _hasAllocatedMemory = false;
                _hasCheckedPatch = false;
                _hasPublishedLoaded = false;
                _hasPublishedFadedIn = false;
                _loaded = false;
                _itemService.Reset();
                IsAttachedText.Text = "Not attached";
                IsAttachedText.Foreground = (SolidColorBrush)Application.Current.Resources["NotAttachedBrush"];
            }
        }

        private void TrySetGameStartPrefs()
        {
            var gameDataPtr = _memoryService.Read<nint>(Offsets.GameDataMan.Base);
            IntPtr inGameTimePtr = gameDataPtr + (int)Offsets.GameDataMan.GameDataOffsets.InGameTime;
            long gameTimeMs = _memoryService.Read<long>(inGameTimePtr);
            if (gameTimeMs < 5000)
            {
                _playerViewModel.TrySetNgPref();
                _itemViewModel.TrySpawnWeaponPref();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _hookManager?.UninstallAllHooks();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;
            }
            else
            {
                DragMove();
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            SettingsManager.Default.WindowLeft = Left;
            SettingsManager.Default.WindowTop = Top;
            SettingsManager.Default.Save();
            _itemService.SignalClose();
            _hookManager.UninstallAllHooks();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void CheckUpdate_Click(object sender, RoutedEventArgs e) => VersionChecker.CheckForUpdates(this, true);
    }
}