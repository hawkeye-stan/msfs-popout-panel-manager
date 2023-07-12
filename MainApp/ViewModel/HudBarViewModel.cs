using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using Prism.Commands;
using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class HudBarViewModel : BaseViewModel
    {
        public ICommand IncreaseSimRateCommand { get; }

        public ICommand DecreaseSimRateCommand { get; }

        public ICommand StartStopTimerCommand { get; }

        public ICommand ResetTimerCommand { get; }

        public ICommand CloseCommand { get; }

        public Guid PanelId { get; set; }

        public PanelConfig PanelConfig => ProfileData.ActiveProfile.PanelConfigs.First(p => p.Id == PanelId);

        public bool IsTimerStarted { get; private set; }

        public DateTime Timer { get; set; }

        public string HudBarTypeText => ProfileData.ActiveProfile.ProfileSetting.HudBarConfig.HudBarType.ToString().Replace("_", " ");

        private Timer _clock;

        public HudBarViewModel(MainOrchestrator orchestrator) : base(orchestrator)
        {
            IncreaseSimRateCommand = new DelegateCommand(OnIncreaseSimRate);
            DecreaseSimRateCommand = new DelegateCommand(OnDecreaseSimRate);
            StartStopTimerCommand = new DelegateCommand(OnStartStopTimer);
            ResetTimerCommand = new DelegateCommand(OnResetTimer);
            CloseCommand = new DelegateCommand(OnClose);

            Timer = new DateTime();

            _clock = new Timer();
            _clock.Interval = 1000;
            _clock.Enabled = false;
            _clock.Elapsed += (sender, e) => Application.Current.Dispatcher.Invoke(() => Timer = Timer.AddSeconds(1));

            Orchestrator.FlightSim.SetHudBarConfig();
        }

        private void OnIncreaseSimRate()
        {
            Orchestrator.FlightSim.IncreaseSimRate();
        }

        private void OnDecreaseSimRate()
        {
            Orchestrator.FlightSim.DecreaseSimRate();
        }

        private void OnStartStopTimer()
        {
            IsTimerStarted = !IsTimerStarted;
            _clock.Enabled = IsTimerStarted;
        }

        private void OnResetTimer()
        {
            IsTimerStarted = false;
            _clock.Enabled = false;
            Timer = new DateTime();
        }

        private void OnClose()
        {
            Orchestrator.FlightSim.StopHudBar();
        }
    }
}
