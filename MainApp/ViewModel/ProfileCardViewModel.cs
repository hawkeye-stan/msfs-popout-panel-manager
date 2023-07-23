using MaterialDesignThemes.Wpf;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class ProfileCardViewModel : BaseViewModel
    {
        public ICommand DeleteProfileCommand { get; }

        public ICommand ToggleAircraftBindingCommand { get; }

        public ICommand ToggleLockProfileCommand { get; }

        public ICommand ToggleEditPanelSourceCommand { get; }

        public ICommand AddPanelCommand { get; }

        public ICommand StartPopOutCommand { get; }

        public ICommand IncludeInGamePanelUpdatedCommand { get; }

        public ICommand AddHudBarUpdatedCommand { get; }

        public ICommand RefocusDisplayUpdatedCommand { get; }

        public ICommand RefocusDisplayRefreshedCommand { get; }

        public DelegateCommand<string> RefocusDisplaySelectionUpdatedCommand { get; }

        public List<string> HudBarTypes => Enum.GetNames(typeof(HudBarType)).Where(x => x != "None").Select(x => x.Replace("_", " ")).ToList();


        public ProfileCardViewModel(MainOrchestrator orchestrator) : base(orchestrator)
        {
            DeleteProfileCommand = new DelegateCommand(OnDeleteProfile);

            ToggleAircraftBindingCommand = new DelegateCommand(OnEditAircraftBinding, () => ProfileData != null && ProfileData.ActiveProfile != null && FlightSimData != null && FlightSimData.HasAircraftName && ProfileData.IsAllowedAddAircraftBinding && FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => FlightSimData.AircraftName)
                                                                                .ObservesProperty(() => FlightSimData.HasAircraftName)
                                                                                .ObservesProperty(() => ProfileData.IsAllowedAddAircraftBinding)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted);

            ToggleLockProfileCommand = new DelegateCommand(OnToggleLockProfile, () => ProfileData != null && ProfileData.ActiveProfile != null && ProfileData.ActiveProfile.PanelConfigs.Count > 0)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.PanelConfigs.Count);

            ToggleEditPanelSourceCommand = new DelegateCommand(OnToggleEditPanelSource, () => ProfileData != null && ProfileData.ActiveProfile != null && ProfileData.ActiveProfile.PanelConfigs.Count > 0 && !ProfileData.ActiveProfile.IsLocked && FlightSimData.IsInCockpit)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.PanelConfigs.Count)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.IsLocked)
                                                                                .ObservesProperty(() => FlightSimData.IsInCockpit);

            AddPanelCommand = new DelegateCommand(OnAddPanel, () => ProfileData != null && ProfileData.ActiveProfile != null && !ProfileData.ActiveProfile.IsLocked)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.IsLocked);

            StartPopOutCommand = new DelegateCommand(OnStartPopOut, () => ProfileData != null && ProfileData.ActiveProfile != null && (ProfileData.ActiveProfile.PanelConfigs.Count > 0 || ProfileData.ActiveProfile.ProfileSetting.IncludeInGamePanels || ProfileData.ActiveProfile.ProfileSetting.HudBarConfig.IsEnabled) && !ProfileData.ActiveProfile.HasUnidentifiedPanelSource && !ProfileData.ActiveProfile.IsEditingPanelSource && FlightSimData.IsInCockpit)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.PanelConfigs.Count)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.ProfileSetting.IncludeInGamePanels)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.ProfileSetting.HudBarConfig.IsEnabled)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.HasUnidentifiedPanelSource)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.IsEditingPanelSource)
                                                                                .ObservesProperty(() => FlightSimData.IsInCockpit);

            IncludeInGamePanelUpdatedCommand = new DelegateCommand(OnIncludeInGamePanelUpdated);

            AddHudBarUpdatedCommand = new DelegateCommand(OnAddHudBarUpdated);

            RefocusDisplayUpdatedCommand = new DelegateCommand(OnRefocusDisplayUpdated);

            RefocusDisplayRefreshedCommand = new DelegateCommand(OnRefocusDisplayRefreshed);

            RefocusDisplaySelectionUpdatedCommand = new DelegateCommand<string>(RefocusDisplaySelectionUpdated);
        }

        private async void OnDeleteProfile()
        {
            var result = await DialogHost.Show(new ConfirmationDialog("WARNING! Are you sure you want to delete the profile?", "Delete"), "RootDialog");

            if (result != null && result.Equals("CONFIRM"))
            {
                Orchestrator.PanelSource.CloseAllPanelSource();
                Orchestrator.PanelConfiguration.EndConfiguration();
                Orchestrator.Profile.DeleteActiveProfile();
            }
        }

        private void OnEditAircraftBinding()
        {
            if (!ProfileData.IsAircraftBoundToProfile)
                Orchestrator.Profile.AddProfileBinding(FlightSimData.AircraftName);
            else
                Orchestrator.Profile.DeleteProfileBinding(FlightSimData.AircraftName);
        }

        private void OnToggleLockProfile()
        {
            if (ProfileData.ActiveProfile.IsLocked)
            {
                Orchestrator.PanelSource.CloseAllPanelSource();
                ProfileData.ActiveProfile.IsEditingPanelSource = false;
            }

            Orchestrator.PanelConfiguration.StartConfiguration();
        }

        private async void OnToggleEditPanelSource()
        {
            if (ProfileData.ActiveProfile.IsEditingPanelSource)
                await Orchestrator.PanelSource.StartEditPanelSources();
            else
                await Orchestrator.PanelSource.EndEditPanelSources();
        }

        private void OnAddPanel()
        {
            Orchestrator.Profile.AddPanel();
        }

        private void OnStartPopOut()
        {
            if (IsDisabledAppInput)
                return;

            Orchestrator.PanelPopOut.ManualPopOut();
        }

        private void OnIncludeInGamePanelUpdated()
        {
            if (ProfileData.ActiveProfile != null && !ProfileData.ActiveProfile.ProfileSetting.IncludeInGamePanels)
                ProfileData.ActiveProfile.PanelConfigs.RemoveAll(p => p.PanelType == PanelType.BuiltInPopout);
        }

        private void OnAddHudBarUpdated()
        {
            if (ProfileData.ActiveProfile == null)
                return;

            if (ProfileData.ActiveProfile.ProfileSetting.HudBarConfig.IsEnabled)
            {
                if (ProfileData.ActiveProfile.PanelConfigs.Any(p => p.PanelType == PanelType.HudBarWindow))
                    return;

                ProfileData.ActiveProfile.PanelConfigs.Add(new PanelConfig
                {
                    PanelName = "HUD Bar",
                    PanelType = PanelType.HudBarWindow,
                    Left = 0,
                    Top = 0,
                    Width = 1920,
                    Height = 40,
                    AutoGameRefocus = false
                });
            }
            else 
            { 
                ProfileData.ActiveProfile.PanelConfigs.RemoveAll(p => p.PanelType == PanelType.HudBarWindow);
            }
        }

        private void OnRefocusDisplayUpdated()
        {
            if (ProfileData.ActiveProfile == null)
                return;

            if (ProfileData.ActiveProfile.ProfileSetting.RefocusOnDisplay.Monitors.Count == 0)
            {
                var monitors = WindowActionManager.GetMonitorsInfo().OrderBy(m => m.Name);

                foreach (var monitor in monitors)
                    ProfileData.ActiveProfile.ProfileSetting.RefocusOnDisplay.Monitors.Add(monitor);
            }

            if (!ProfileData.ActiveProfile.ProfileSetting.RefocusOnDisplay.IsEnabled)
            {
                ProfileData.ActiveProfile.PanelConfigs.RemoveAll(p => p.PanelType == PanelType.RefocusDisplay);
                ProfileData.ActiveProfile.ProfileSetting.RefocusOnDisplay.Monitors.ToList().ForEach(p => p.IsSelected = false);
            }
        }

        private void OnRefocusDisplayRefreshed()
        {
            if (ProfileData.ActiveProfile == null)
                return;

            ProfileData.ActiveProfile.ProfileSetting.RefocusOnDisplay.Monitors.Clear();
            ProfileData.ActiveProfile.PanelConfigs.RemoveAll(p => p.PanelType == PanelType.RefocusDisplay);

            var monitors = WindowActionManager.GetMonitorsInfo().OrderBy(m => m.Name);

            foreach (var monitor in monitors)
                ProfileData.ActiveProfile.ProfileSetting.RefocusOnDisplay.Monitors.Add(monitor);
        }

        private void RefocusDisplaySelectionUpdated(string arg)
        {
            if (ProfileData.ActiveProfile == null)
                return;

            var monitor = ProfileData.ActiveProfile.ProfileSetting.RefocusOnDisplay.Monitors.FirstOrDefault(m => m.Name == arg);

            if (monitor == null)
                return;

            if(!monitor.IsSelected)
            {
                ProfileData.ActiveProfile.PanelConfigs.RemoveAll(p => p.PanelName == arg && p.PanelType == PanelType.RefocusDisplay);
            }
            else
            {
                ProfileData.ActiveProfile.PanelConfigs.Add(new PanelConfig
                {
                    PanelName = arg,
                    PanelType = PanelType.RefocusDisplay,
                    Left = monitor.X,
                    Top = monitor.Y,
                    Width = monitor.Width,
                    Height = monitor.Height,
                    HideTitlebar = true,
                    FullScreen = true,
                    TouchEnabled = true    
                });
            }
        }
    }
}