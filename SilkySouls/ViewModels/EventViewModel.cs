using System.Windows.Input;
using System.Windows.Media;
using SilkySouls.Core;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Services;

namespace SilkySouls.ViewModels
{
    public class EventViewModel : BaseViewModel
    {
        private readonly EventService _eventService;

        public EventViewModel(EventService eventService, IStateService stateService)
        {
            _eventService = eventService;

            stateService.Subscribe(State.Loaded, OnLoaded);
            stateService.Subscribe(State.NotLoaded, OnNotLoaded);

            SetFlagCommand = new DelegateCommand(SetFlag);
            GetEventCommand = new DelegateCommand(GetEvent);
            UnlockKalameetCommand = new DelegateCommand(() => _eventService.SetMultipleEventsOn(GameIdsOld.EventFlags.UnlockKalameet));
            RingGargBellCommand = new DelegateCommand(() => _eventService.RingGargBell());
            RingQuelaggBellCommand = new DelegateCommand(() => _eventService.RingQuelaagBell());
            OpenSensCommand = new DelegateCommand(() => _eventService.OpenSensGate(GameIdsOld.EventFlags.Sens));
            PlaceLordVesselCommand = new DelegateCommand(() => _eventService.PlaceLordVessel());
            NewLondoNoWaterCommand = new DelegateCommand(() => _eventService.SetEvent(GameIdsOld.EventFlags.NewLondoWater, true));
            LaurentiusToFirelinkCommand = new DelegateCommand(() => _eventService.SetEvent(GameIdsOld.EventFlags.LaurentiusToFirelink, true));
            LoganToFirelinkCommand = new DelegateCommand(() => _eventService.SetMultipleEventsOn(GameIdsOld.EventFlags.LoganToFirelink));
            GriggsToFirelinkCommand = new DelegateCommand(() => _eventService.SetMultipleEventsOn(GameIdsOld.EventFlags.GriggsToFirelink));
        }

        #region Commands

        public ICommand SetFlagCommand { get; }
        public ICommand GetEventCommand { get; }
        public ICommand UnlockKalameetCommand { get; }
        public ICommand RingGargBellCommand { get; }
        public ICommand RingQuelaggBellCommand { get; }
        public ICommand OpenSensCommand { get; }
        public ICommand PlaceLordVesselCommand { get; }
        public ICommand NewLondoNoWaterCommand { get; }
        public ICommand LaurentiusToFirelinkCommand { get; }
        public ICommand LoganToFirelinkCommand { get; }
        public ICommand GriggsToFirelinkCommand { get; }

        #endregion

        #region Properties

        private bool _areOptionsEnabled;

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }

        private bool _isDisableEventsEnabled;

        public bool IsDisableEventsEnabled
        {
            get => _isDisableEventsEnabled;
            set
            {
                if (!SetProperty(ref _isDisableEventsEnabled, value)) return;
                _eventService.ToggleDisableEvents(_isDisableEventsEnabled);
            }
        }

        private string _setFlagId;

        public string SetFlagId
        {
            get => _setFlagId;
            set => SetProperty(ref _setFlagId, value);
        }

        private int _flagStateIndex;

        public int FlagStateIndex
        {
            get => _flagStateIndex;
            set => SetProperty(ref _flagStateIndex, value);
        }

        private string _getFlagId;

        public string GetFlagId
        {
            get => _getFlagId;
            set => SetProperty(ref _getFlagId, value);
        }

        private string _eventStatusText;

        public string EventStatusText
        {
            get => _eventStatusText;
            set => SetProperty(ref _eventStatusText, value);
        }

        private Brush _eventStatusColor;

        public Brush EventStatusColor
        {
            get => _eventStatusColor;
            set => SetProperty(ref _eventStatusColor, value);
        }

        #endregion

        #region Private Methods

        private void SetFlag()
        {
            if (string.IsNullOrWhiteSpace(SetFlagId))
                return;

            string trimmedFlagId = SetFlagId.Trim();

            if (!int.TryParse(trimmedFlagId, out int flagIdValue) || flagIdValue <= 0)
                return;
            _eventService.SetEvent(flagIdValue, FlagStateIndex == 0);
        }

        private void GetEvent()
        {
            if (string.IsNullOrWhiteSpace(GetFlagId))
                return;

            string trimmedFlagId = GetFlagId.Trim();

            if (!int.TryParse(trimmedFlagId, out int flagIdValue) || flagIdValue <= 0)
                return;

            if (_eventService.GetEvent(flagIdValue))
            {
                EventStatusText = "True";
                EventStatusColor = Brushes.Chartreuse;
            }
            else
            {
                EventStatusText = "False";
                EventStatusColor = Brushes.Red;
            }
        }

        private void OnLoaded()
        {
            AreOptionsEnabled = true;
        }

        private void OnNotLoaded()
        {
            AreOptionsEnabled = false;
            IsDisableEventsEnabled = false;
        }

        #endregion
    }
}
