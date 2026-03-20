using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
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
        private readonly TravelViewModel _travelViewModel;
        private readonly EventViewModel _eventViewModel;
        private readonly UtilityViewModel _utilityViewModel;
        private readonly EnemyViewModel _enemyViewModel;
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

            _hookManager = new HookManager(_memoryService);
            var hotkeyManager = new HotkeyManager(_memoryService);
            _aobScanner = new AoBScanner(_memoryService);
            var playerService = new PlayerService(_memoryService);
            var travelService = new TravelService(_memoryService, _hookManager);
            var eventService = new EventService(_memoryService, _hookManager);
            var utilityService = new UtilityService(_memoryService, _hookManager);
            var enemyService = new EnemyService(_memoryService, _hookManager, _aobScanner);
            IParamService paramService = new ParamService(_memoryService);
            _itemService = new ItemService(_memoryService);
            var settingsService = new SettingsService(_memoryService);

            _playerViewModel = new PlayerViewModel(playerService, hotkeyManager);
            _utilityViewModel = new UtilityViewModel(utilityService, hotkeyManager, _playerViewModel, paramService);
            _travelViewModel = new TravelViewModel(travelService, hotkeyManager, _utilityViewModel);
            _eventViewModel = new EventViewModel(eventService);
            _enemyViewModel = new EnemyViewModel(enemyService, hotkeyManager);
            _itemViewModel = new ItemViewModel(_itemService);
            _settingsViewModel = new SettingsViewModel(settingsService, hotkeyManager);

            var playerTab = new PlayerTab(_playerViewModel);
            var travelTab = new TravelTab(_travelViewModel);
            var eventTab = new EventTab(_eventViewModel);
            var utilityTab = new UtilityTab(_utilityViewModel);
            var enemyTab = new EnemyTab(_enemyViewModel);
            var itemTab = new ItemTab(_itemViewModel);
            var settingsTab = new SettingsTab(_settingsViewModel);

            MainTabControl.Items.Add(new TabItem { Header = "Player", Content = playerTab });
            MainTabControl.Items.Add(new TabItem { Header = "Travel", Content = travelTab });
            MainTabControl.Items.Add(new TabItem { Header = "Event", Content = eventTab });
            MainTabControl.Items.Add(new TabItem { Header = "Utility", Content = utilityTab });
            MainTabControl.Items.Add(new TabItem { Header = "Enemies", Content = enemyTab });
            MainTabControl.Items.Add(new TabItem { Header = "Items", Content = itemTab });
            MainTabControl.Items.Add(new TabItem { Header = "Settings", Content = settingsTab });

            _settingsViewModel.ApplyStartUpOptions();
            Closing += MainWindow_Closing;
            
            _gameLoadedTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1)
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
        private bool _hasScanned;
        private bool _hasAllocatedMemory;

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_memoryService.IsAttached)
            {
                IsAttachedText.Text = "Attached to game";
                IsAttachedText.Foreground = (SolidColorBrush)Application.Current.Resources["AttachedBrush"];
                
                if (!_hasScanned)
                {
                    _aobScanner.Scan();
                    _hasScanned = true;
                }

                if (!_hasAllocatedMemory)
                {
                    _memoryService.AllocCodeCave();
                    Console.WriteLine($"Code cave: 0x{CodeCaveOffsets.Base.ToInt64():X}");
                    _hasAllocatedMemory = true;
                }
                
                _utilityViewModel.TryRestoreAttachedFeatures();
                
                if (_stateService.IsLoaded())
                {
                    if (_loaded) return;
                    _loaded = true;
                    TryEnableFeatures();
                    TrySetGameStartPrefs();
                    _settingsViewModel.ApplyLoadedOptions();
                }
                else if (_loaded)
                {
                    DisableFeatures();
                    _loaded = false;
                }
            }
            else
            {
                _hookManager.ClearHooks();
                DisableFeatures();
                _utilityViewModel.ResetAttached();
                _settingsViewModel.ResetAttached();
                _hasAllocatedMemory = false;
                _loaded = false;
                _itemService.Reset();
                IsAttachedText.Text = "Not attached";
                IsAttachedText.Foreground = (SolidColorBrush)Application.Current.Resources["NotAttachedBrush"];
            }
        }

        private void TryEnableFeatures()
        {
            _playerViewModel.TryEnableActiveOptions();
            _eventViewModel.TryEnableActiveOptions();
            _utilityViewModel.TryEnableActiveOptions();
            _enemyViewModel.TryEnableActiveOptions();
            _itemViewModel.TryEnableActiveOptions();
            _travelViewModel.TryEnableFeatures();
        }

        private void DisableFeatures()
        {
            _travelViewModel.DisableFeatures();
            _eventViewModel.DisableFeatures();
            _playerViewModel.DisableButtons();
            _utilityViewModel.DisableButtons();
            _enemyViewModel.DisableButtons();
            _itemViewModel.DisableButtons();
        }

        private void TrySetGameStartPrefs()
        {
            var gameDataPtr = _memoryService.Read<nint>(Offsets.GameDataMan.Base);
            IntPtr inGameTimePtr = (IntPtr)(gameDataPtr + (int)Offsets.GameDataMan.GameDataOffsets.InGameTime);
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