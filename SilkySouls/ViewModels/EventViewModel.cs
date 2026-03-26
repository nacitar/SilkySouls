using System.Windows.Input;
using System.Windows.Media;
using SilkySouls.Core;
using SilkySouls.Enums;
using SilkySouls.GameIds;
using SilkySouls.Interfaces;
using SilkySouls.Services;

namespace SilkySouls.ViewModels
{
    public class EventViewModel : BaseViewModel
    {
        private readonly IEventService _eventService;

        public EventViewModel(IEventService eventService, IStateService stateService)
        {
            _eventService = eventService;

            stateService.Subscribe(State.Loaded, OnLoaded);
            stateService.Subscribe(State.NotLoaded, OnNotLoaded);

            SetFlagCommand = new DelegateCommand(SetFlag);
            GetEventCommand = new DelegateCommand(GetEvent);
            UnlockKalameetCommand = new DelegateCommand(UnlockKalameet);
            RingGargBellCommand = new DelegateCommand(RingGargBell);
            RingQuelaggBellCommand = new DelegateCommand(RingQuelaagBell);
            OpenSensCommand = new DelegateCommand(OpenSens);
            PlaceLordVesselCommand = new DelegateCommand(PlaceLordVessel);
            NewLondoNoWaterCommand = new DelegateCommand(NewLondoNoWater);
            LaurentiusToFirelinkCommand = new DelegateCommand(LaurentiusToFirelink);
            LoganToFirelinkCommand = new DelegateCommand(LoganToFirelink);
            GriggsToFirelinkCommand = new DelegateCommand(GriggsToFirelink);
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

        private void UnlockKalameet()
        {
            foreach (var eventId in EventFlags.UnlockKalameet)
            {
                _eventService.SetEvent(eventId, true);
            }
        }

        private void RingGargBell()
        {
            _eventService.SetEvent(EventFlags.GargBell, true);

            if (_eventService.GetEvent(EventFlags.QuelaagBell))
            {
                _eventService.SetEvent(EventFlags.Sens, true);
            }
        }

        private void RingQuelaagBell()
        {
            _eventService.SetEvent(EventFlags.QuelaagBell, true);

            if (_eventService.GetEvent(EventFlags.GargBell))
            {
                _eventService.SetEvent(EventFlags.Sens, true);
            }
        }

        private void OpenSens() => _eventService.OpenSensGate();
        private void PlaceLordVessel() => _eventService.PlaceLordVessel();

        private void NewLondoNoWater() => _eventService.SetEvent(EventFlags.NewLondoWater, true);

        private void LaurentiusToFirelink() => _eventService.SetEvent(EventFlags.LaurentiusToFirelink, true);

        private void LoganToFirelink()
        {
            foreach (var eventId in EventFlags.LoganToFirelink)
            {
                _eventService.SetEvent(eventId, true);
            }
        }

        private void GriggsToFirelink()
        {
            foreach (var eventId in EventFlags.GriggsToFirelink)
            {
                _eventService.SetEvent(eventId, true);
            }
        }

        #endregion
    }
}