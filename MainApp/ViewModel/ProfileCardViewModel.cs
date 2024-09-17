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
using MSFSPopoutPanelManager.MainApp.AppUserControl.Dialog;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class ProfileCardViewModel : BaseViewModel
    {
        private readonly ProfileOrchestrator _profileOrchestrator;
        private readonly PanelSourceOrchestrator _panelSourceOrchestrator;
        private readonly PanelConfigurationOrchestrator _panelConfigurationOrchestrator;
        private readonly PanelPopOutOrchestrator _panelPopOutOrchestrator;

        public ICommand DeleteProfileCommand { get; }

        public ICommand ToggleAircraftBindingCommand { get; }

        public ICommand ToggleLockProfileCommand { get; }

        public ICommand ToggleEditPanelSourceCommand { get; }

        public ICommand AddPanelCommand { get; }

        public ICommand StartPopOutCommand { get; }

        public ICommand ClosePopOutCommand { get; }

        public ICommand IncludeInGamePanelUpdatedCommand { get; }

        public ICommand AddHudBarUpdatedCommand { get; }

        public ICommand RefocusDisplayUpdatedCommand { get; }

        public ICommand AddNumPadUpdatedCommand { get; }

        public ICommand AddSwitchWindowUpdatedCommand { get; }

        public ICommand RefocusDisplayRefreshedCommand { get; }

        public DelegateCommand<string> RefocusDisplaySelectionUpdatedCommand { get; }

        public List<string> HudBarTypes => Enum.GetNames(typeof(HudBarType)).Where(x => x != "None").Select(x => x.Replace("_", " ")).ToList();


        public ProfileCardViewModel(SharedStorage sharedStorage, ProfileOrchestrator profileOrchestrator, PanelSourceOrchestrator panelSourceOrchestrator, PanelConfigurationOrchestrator panelConfigurationOrchestrator, PanelPopOutOrchestrator panelPopOutOrchestrator) : base(sharedStorage)
        {
            _profileOrchestrator = profileOrchestrator;
            _panelSourceOrchestrator = panelSourceOrchestrator;
            _panelConfigurationOrchestrator = panelConfigurationOrchestrator;
            _panelPopOutOrchestrator = panelPopOutOrchestrator;

            DeleteProfileCommand = new DelegateCommand(OnDeleteProfile);

            ToggleAircraftBindingCommand = new DelegateCommand(OnEditAircraftBinding, () => ProfileData != null && ActiveProfile != null && FlightSimData != null && FlightSimData.HasAircraftName && ProfileData.IsAllowedAddAircraftBinding && FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => ActiveProfile)
                                                                                .ObservesProperty(() => FlightSimData.AircraftName)
                                                                                .ObservesProperty(() => FlightSimData.HasAircraftName)
                                                                                .ObservesProperty(() => ProfileData.IsAllowedAddAircraftBinding)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted);

            ToggleLockProfileCommand = new DelegateCommand(OnToggleLockProfile, () => ProfileData != null && ActiveProfile != null && ActiveProfile.PanelConfigs.Count > 0)
                                                                                .ObservesProperty(() => ActiveProfile)
                                                                                .ObservesProperty(() => ActiveProfile.PanelConfigs.Count);

            ToggleEditPanelSourceCommand = new DelegateCommand(OnToggleEditPanelSource, () => ProfileData != null && ActiveProfile != null && ActiveProfile.PanelConfigs.Count > 0 && !ActiveProfile.IsLocked && FlightSimData.IsInCockpit)
                                                                                .ObservesProperty(() => ActiveProfile)
                                                                                .ObservesProperty(() => ActiveProfile.PanelConfigs.Count)
                                                                                .ObservesProperty(() => ActiveProfile.IsLocked)
                                                                                .ObservesProperty(() => FlightSimData.IsInCockpit);

            AddPanelCommand = new DelegateCommand(OnAddPanel, () => ProfileData != null && ActiveProfile != null && !ActiveProfile.IsLocked && FlightSimData.IsInCockpit)
                                                                                .ObservesProperty(() => ActiveProfile)
                                                                                .ObservesProperty(() => ActiveProfile.IsLocked)
                                                                                .ObservesProperty(() => FlightSimData.IsInCockpit);

            StartPopOutCommand = new DelegateCommand(OnStartPopOut, () => ProfileData != null && ActiveProfile != null && (ActiveProfile.PanelConfigs.Count > 0 || ActiveProfile.ProfileSetting.IncludeInGamePanels || ActiveProfile.ProfileSetting.HudBarConfig.IsEnabled) && !ActiveProfile.HasUnidentifiedPanelSource && !ActiveProfile.IsEditingPanelSource && !ActiveProfile.IsDisabledStartPopOut && FlightSimData.IsInCockpit)
                                                                                .ObservesProperty(() => ActiveProfile)
                                                                                .ObservesProperty(() => ActiveProfile.PanelConfigs.Count)
                                                                                .ObservesProperty(() => ActiveProfile.ProfileSetting.IncludeInGamePanels)
                                                                                .ObservesProperty(() => ActiveProfile.ProfileSetting.HudBarConfig.IsEnabled)
                                                                                .ObservesProperty(() => ActiveProfile.HasUnidentifiedPanelSource)
                                                                                .ObservesProperty(() => ActiveProfile.IsEditingPanelSource)
                                                                                .ObservesProperty(() => ActiveProfile.IsDisabledStartPopOut)
                                                                                .ObservesProperty(() => FlightSimData.IsInCockpit);

            ClosePopOutCommand = new DelegateCommand(OnClosePopOut, () => ProfileData != null && ActiveProfile != null && ActiveProfile.PanelConfigs.Count > 0 && ActiveProfile.IsPoppedOut && FlightSimData.IsInCockpit)
                                                                                .ObservesProperty(() => ActiveProfile)
                                                                                .ObservesProperty(() => ActiveProfile.PanelConfigs.Count)
                                                                                .ObservesProperty(() => FlightSimData.IsInCockpit)
                                                                                .ObservesProperty(() => ActiveProfile.IsPoppedOut);

            IncludeInGamePanelUpdatedCommand = new DelegateCommand(OnIncludeInGamePanelUpdated);

            AddHudBarUpdatedCommand = new DelegateCommand(OnAddHudBarUpdated);

            AddNumPadUpdatedCommand = new DelegateCommand(OnAddNumPadUpdated);
#if LOCAL
            AddSwitchWindowUpdatedCommand = new DelegateCommand(OnAddSwitchWindowUpdated);
#endif
            RefocusDisplayUpdatedCommand = new DelegateCommand(OnRefocusDisplayUpdated);

            RefocusDisplayRefreshedCommand = new DelegateCommand(OnRefocusDisplayRefreshed);

            RefocusDisplaySelectionUpdatedCommand = new DelegateCommand<string>(RefocusDisplaySelectionUpdated);
        }

        private async void OnDeleteProfile()
        {
            var result = await DialogHost.Show(new ConfirmationDialog("WARNING! Are you sure you want to delete the profile?", "Delete"), "RootDialog");

            if (result != null && result.Equals("CONFIRM"))
            {
                _panelSourceOrchestrator.CloseAllPanelSource();
                _panelConfigurationOrchestrator.EndConfiguration();
                _profileOrchestrator.DeleteActiveProfile();
            }
        }

        private void OnEditAircraftBinding()
        {
            if (!ProfileData.IsAircraftBoundToProfile)
                _profileOrchestrator.AddProfileBinding(FlightSimData.AircraftName);
            else
                _profileOrchestrator.DeleteProfileBinding(FlightSimData.AircraftName);
        }

        private void OnToggleLockProfile()
        {
            if (ActiveProfile.IsLocked)
            {
                _panelSourceOrchestrator.CloseAllPanelSource();

                foreach (var panelConfig in ActiveProfile.PanelConfigs)
                {
                    panelConfig.IsSelectedPanelSource = false;
                    panelConfig.IsEditingPanel = false;
                }

                ActiveProfile.IsSelectingPanelSource = false;
                ActiveProfile.IsEditingPanelSource = false;
            }

            _panelConfigurationOrchestrator.StartConfiguration();
        }

        private async void OnToggleEditPanelSource()
        {
            if (ActiveProfile.IsEditingPanelSource)
                await _panelSourceOrchestrator.StartEditPanelSources();
            else
                await _panelSourceOrchestrator.EndEditPanelSources();
        }

        private async void OnAddPanel()
        {
            _profileOrchestrator.AddPanel();

            ActiveProfile.IsEditingPanelSource = true;
            await _panelSourceOrchestrator.StartEditPanelSources();
        }

        private async void OnStartPopOut()
        {
            if (ActiveProfile.IsDisabledStartPopOut || !FlightSimData.IsInCockpit)
                return;

            await _panelPopOutOrchestrator.ManualPopOut();
        }

        private void OnClosePopOut()
        {
            if (ActiveProfile.IsDisabledStartPopOut || !FlightSimData.IsInCockpit)
                return;

            _panelPopOutOrchestrator.ClosePopOut();
        }

        private void OnIncludeInGamePanelUpdated()
        {
            if (ActiveProfile != null && !ActiveProfile.ProfileSetting.IncludeInGamePanels)
                ActiveProfile.PanelConfigs.RemoveAll(p => p.PanelType == PanelType.BuiltInPopout);
        }

        private void OnAddHudBarUpdated()
        {
            if (ActiveProfile == null)
                return;

            if (ActiveProfile.ProfileSetting.HudBarConfig.IsEnabled)
            {
                if (ActiveProfile.PanelConfigs.Any(p => p.PanelType == PanelType.HudBarWindow))
                    return;

                ActiveProfile.PanelConfigs.Add(new PanelConfig
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
                ActiveProfile.PanelConfigs.RemoveAll(p => p.PanelType == PanelType.HudBarWindow);
            }
        }

        private void OnAddNumPadUpdated()
        {
            if (ActiveProfile == null)
                return;

            if (ActiveProfile.ProfileSetting.NumPadConfig.IsEnabled)
            {
                if (ActiveProfile.PanelConfigs.Any(p => p.PanelType == PanelType.NumPadWindow))
                    return;

                ActiveProfile.PanelConfigs.Add(new PanelConfig
                {
                    PanelName = "Virtual NumPad",
                    PanelType = PanelType.NumPadWindow,
                    Left = 0,
                    Top = 0,
                    Width = 0,
                    Height = 0,
                    AutoGameRefocus = false
                });
            }
            else
            {
                ActiveProfile.PanelConfigs.RemoveAll(p => p.PanelType == PanelType.NumPadWindow);
            }
        }

        private void OnAddSwitchWindowUpdated()
        {
            if (ActiveProfile == null)
                return;

            if (ActiveProfile.ProfileSetting.SwitchWindowConfig.IsEnabled)
            {
                if (ActiveProfile.PanelConfigs.Any(p => p.PanelType == PanelType.SwitchWindow))
                    return;

                ActiveProfile.PanelConfigs.Add(new PanelConfig
                {
                    PanelName = "Switch Window",
                    PanelType = PanelType.SwitchWindow,
                    Left = 0,
                    Top = 0,
                    Width = 0,
                    Height = 0,
                    AutoGameRefocus = false
                });
            }
            else
            {
                ActiveProfile.PanelConfigs.RemoveAll(p => p.PanelType == PanelType.SwitchWindow);
                ActiveProfile.ProfileSetting.SwitchWindowConfig.Panels = null;
            }
        }

        private void OnRefocusDisplayUpdated()
        {
            if (ActiveProfile == null)
                return;

            if (ActiveProfile.ProfileSetting.RefocusOnDisplay.Monitors.Count == 0)
            {
                var monitors = WindowActionManager.GetMonitorsInfo().OrderBy(m => m.Name);

                foreach (var monitor in monitors)
                    ActiveProfile.ProfileSetting.RefocusOnDisplay.Monitors.Add(monitor);
            }

            if (!ActiveProfile.ProfileSetting.RefocusOnDisplay.IsEnabled)
            {
                ActiveProfile.PanelConfigs.RemoveAll(p => p.PanelType == PanelType.RefocusDisplay);
                ActiveProfile.ProfileSetting.RefocusOnDisplay.Monitors.ToList().ForEach(p => p.IsSelected = false);
            }
        }

        private void OnRefocusDisplayRefreshed()
        {
            if (ActiveProfile == null)
                return;

            ActiveProfile.ProfileSetting.RefocusOnDisplay.Monitors.Clear();
            ActiveProfile.PanelConfigs.RemoveAll(p => p.PanelType == PanelType.RefocusDisplay);

            var monitors = WindowActionManager.GetMonitorsInfo().OrderBy(m => m.Name);

            foreach (var monitor in monitors)
                ActiveProfile.ProfileSetting.RefocusOnDisplay.Monitors.Add(monitor);
        }

        private void RefocusDisplaySelectionUpdated(string arg)
        {
            if (ActiveProfile == null)
                return;

            var monitor = ActiveProfile.ProfileSetting.RefocusOnDisplay.Monitors.FirstOrDefault(m => m.Name == arg);

            if (monitor == null)
                return;

            if (!monitor.IsSelected)
            {
                ActiveProfile.PanelConfigs.RemoveAll(p => p.PanelName == arg && p.PanelType == PanelType.RefocusDisplay);
            }
            else
            {
                ActiveProfile.PanelConfigs.Add(new PanelConfig
                {
                    PanelName = arg,
                    PanelType = PanelType.RefocusDisplay,
                    Left = monitor.X,
                    Top = monitor.Y,
                    Width = monitor.Width,
                    Height = monitor.Height,
                    TouchEnabled = true
                });
            }
        }
    }
}