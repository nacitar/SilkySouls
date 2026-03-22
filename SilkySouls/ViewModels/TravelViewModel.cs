using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using SilkySouls.Core;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Models;
using SilkySouls.Utilities;

namespace SilkySouls.ViewModels
{
    public class TravelViewModel : BaseViewModel
    {
        private readonly ITravelService _travelService;
        private readonly HotkeyManager _hotkeyManager;
        private readonly UtilityViewModel _utilityViewModel;

        private Dictionary<string, List<WarpLocation>> _locationDict;
        private List<WarpLocation> _allLocations;

        private string _preSearchMainArea;
        private readonly ObservableCollection<WarpLocation> _searchResultsCollection = new();

        public TravelViewModel(ITravelService travelService,
            HotkeyManager hotkeyManager,
            UtilityViewModel utilityViewModel, IStateService stateService)
        {
            _travelService = travelService;
            _hotkeyManager = hotkeyManager;
            _utilityViewModel = utilityViewModel;

            stateService.Subscribe(State.Loaded, OnLoaded);
            stateService.Subscribe(State.NotLoaded, OnNotLoaded);

            _mainAreas = new ObservableCollection<string>();
            _areaLocations = new ObservableCollection<WarpLocation>();

            WarpCommand = new DelegateCommand(Warp);
            UnlockAllBonfiresCommand = new DelegateCommand(() => _travelService.UnlockBonfireWarps());

            LoadLocations();
            RegisterHotkeys();
        }

        #region Commands

        public ICommand WarpCommand { get; }
        public ICommand UnlockAllBonfiresCommand { get; }

        #endregion

        #region Properties

        private bool _areOptionsEnabled;

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }

        private ObservableCollection<string> _mainAreas;

        public ObservableCollection<string> MainAreas
        {
            get => _mainAreas;
            private set => SetProperty(ref _mainAreas, value);
        }

        private ObservableCollection<WarpLocation> _areaLocations;

        public ObservableCollection<WarpLocation> AreaLocations
        {
            get => _areaLocations;
            set => SetProperty(ref _areaLocations, value);
        }

        private string _selectedMainArea;

        public string SelectedMainArea
        {
            get => _selectedMainArea;
            set
            {
                if (!SetProperty(ref _selectedMainArea, value) || value == null) return;

                if (_isSearchActive)
                {
                    IsSearchActive = false;
                    _searchText = string.Empty;
                    OnPropertyChanged(nameof(SearchText));
                    _preSearchMainArea = null;
                }

                UpdateLocationsList();
            }
        }

        private WarpLocation _selectedWarpLocation;

        public WarpLocation SelectedWarpLocation
        {
            get => _selectedWarpLocation;
            set => SetProperty(ref _selectedWarpLocation, value);
        }

        private bool _isSearchActive;

        public bool IsSearchActive
        {
            get => _isSearchActive;
            private set => SetProperty(ref _isSearchActive, value);
        }

        private string _searchText = string.Empty;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (!SetProperty(ref _searchText, value)) return;

                if (string.IsNullOrEmpty(value))
                {
                    _isSearchActive = false;

                    if (_preSearchMainArea != null)
                    {
                        _selectedMainArea = _preSearchMainArea;
                        UpdateLocationsList();
                        _preSearchMainArea = null;
                    }
                }
                else
                {
                    if (!_isSearchActive)
                    {
                        _preSearchMainArea = SelectedMainArea;
                        _isSearchActive = true;
                    }

                    ApplyFilter();
                }
            }
        }

        #endregion

        #region Private Methods

        private void Warp()
        {
            if (SelectedWarpLocation == null) return;
            if (_utilityViewModel.IsNoClipEnabled) _utilityViewModel.DisableNoClip();

            if (SelectedWarpLocation.HasCoordinates)
            {
                _ = Task.Run(() => _travelService.WarpWithCoords(
                    SelectedWarpLocation.Coords.Value,
                    SelectedWarpLocation.Angle,
                    SelectedWarpLocation.Id)
                );
            }
            else
            {
                _travelService.Warp(SelectedWarpLocation.Id);
            }
        }

        private void OnNotLoaded()
        {
            AreOptionsEnabled = false;
        }

        private void OnLoaded()
        {
            AreOptionsEnabled = true;
        }

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction(HotkeyActions.Warp, Warp);
        }

        private void LoadLocations()
        {
            _locationDict = DataLoader.GetLocationDict();

            _allLocations = _locationDict.Values.SelectMany(x => x).ToList();

            foreach (var area in _locationDict.Keys)
            {
                _mainAreas.Add(area);
            }

            SelectedMainArea = _mainAreas.FirstOrDefault();
        }

        private void UpdateLocationsList()
        {
            if (string.IsNullOrEmpty(SelectedMainArea) || !_locationDict.ContainsKey(SelectedMainArea))
            {
                AreaLocations = new ObservableCollection<WarpLocation>();
                return;
            }

            AreaLocations = new ObservableCollection<WarpLocation>(_locationDict[SelectedMainArea]);
            SelectedWarpLocation = AreaLocations.FirstOrDefault();
        }

        private void ApplyFilter()
        {
            _searchResultsCollection.Clear();
            var searchTextLower = SearchText.ToLower();

            foreach (var location in _allLocations)
            {
                if (location.Name.ToLower().Contains(searchTextLower) ||
                    location.MainArea.ToLower().Contains(searchTextLower))
                {
                    _searchResultsCollection.Add(location);
                }
            }

            AreaLocations = new ObservableCollection<WarpLocation>(_searchResultsCollection);
            SelectedWarpLocation = AreaLocations.FirstOrDefault();
        }

        #endregion
    }
}
