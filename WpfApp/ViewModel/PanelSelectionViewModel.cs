using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.Shared;
using Prism.Commands;
using System;
using System.Linq;

namespace MSFSPopoutPanelManager.WpfApp.ViewModel
{
    public class PanelSelectionViewModel : ObservableObject
    {
        private MainOrchestrator _orchestrator;

        public PanelSelectionViewModel(MainOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;

            AddProfileCommand = new DelegateCommand(OnAddProfile, () => FlightSimData.HasCurrentMsfsAircraft && FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => FlightSimData.HasCurrentMsfsAircraft)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted);

            DeleteProfileCommand = new DelegateCommand(OnDeleteProfile, () => ProfileData.HasActiveProfile && FlightSimData.HasCurrentMsfsAircraft && FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => ProfileData.HasActiveProfile)
                                                                                .ObservesProperty(() => FlightSimData.HasCurrentMsfsAircraft)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted);

            ChangeProfileCommand = new DelegateCommand<object>(OnChangeProfile, (obj) => FlightSimData.HasCurrentMsfsAircraft && FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => FlightSimData.HasCurrentMsfsAircraft)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted);

            AddProfileBindingCommand = new DelegateCommand(OnAddProfileBinding, () => ProfileData.HasActiveProfile && FlightSimData.HasCurrentMsfsAircraft && ProfileData.IsAllowedAddAircraftBinding && FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => FlightSimData.HasCurrentMsfsAircraft)
                                                                                .ObservesProperty(() => ProfileData.HasActiveProfile)
                                                                                .ObservesProperty(() => ProfileData.IsAllowedAddAircraftBinding)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted);

            DeleteProfileBindingCommand = new DelegateCommand(OnDeleteProfileBinding, () => ProfileData.HasActiveProfile && ProfileData.IsAllowedDeleteAircraftBinding && ProfileData.ActiveProfile.BindingAircrafts.Count > 0 && FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => ProfileData.HasActiveProfile)
                                                                                .ObservesProperty(() => ProfileData.IsAllowedDeleteAircraftBinding)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.BindingAircrafts.Count)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted);

            SetPowerOnRequiredCommand = new DelegateCommand(() => ProfileData.WriteProfiles(), () => FlightSimData.HasCurrentMsfsAircraft && ProfileData.HasActiveProfile && FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => FlightSimData.HasCurrentMsfsAircraft)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted);

            StartPanelSelectionCommand = new DelegateCommand(OnStartPanelSelection, () => FlightSimData.HasCurrentMsfsAircraft && ProfileData.HasActiveProfile && ProfileData.ActiveProfile != null && FlightSimData.IsSimulatorStarted && FlightSimData.IsInCockpit)
                                                                                .ObservesProperty(() => FlightSimData.HasCurrentMsfsAircraft)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => FlightSimData.IsInCockpit);

            StartPopOutCommand = new DelegateCommand(OnStartPopOut, () => FlightSimData.HasCurrentMsfsAircraft && ProfileData.HasActiveProfile && (ProfileData.ActiveProfile.PanelSourceCoordinates.Count > 0 || ProfileData.ActiveProfile.TouchPanelBindings.Count > 0) && FlightSimData.IsSimulatorStarted && FlightSimData.IsInCockpit)
                                                                                .ObservesProperty(() => FlightSimData.HasCurrentMsfsAircraft)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.PanelSourceCoordinates.Count)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.TouchPanelBindings.Count)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => FlightSimData.IsInCockpit);

            SaveAutoPanningCameraCommand = new DelegateCommand(OnSaveAutoPanningCamera, () => FlightSimData.HasCurrentMsfsAircraft && ProfileData.HasActiveProfile && ProfileData.ActiveProfile.PanelSourceCoordinates.Count > 0 && FlightSimData.IsSimulatorStarted && FlightSimData.IsInCockpit)
                                                                                .ObservesProperty(() => FlightSimData.HasCurrentMsfsAircraft)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.PanelSourceCoordinates.Count)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => FlightSimData.IsInCockpit);

            EditPanelSourceCommand = new DelegateCommand(EditPanelSource, () => FlightSimData.HasCurrentMsfsAircraft && ProfileData.HasActiveProfile && ProfileData.ActiveProfile.PanelSourceCoordinates.Count > 0 && FlightSimData.IsSimulatorStarted && FlightSimData.IsInCockpit)
                                                                                .ObservesProperty(() => FlightSimData.HasCurrentMsfsAircraft)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.PanelSourceCoordinates.Count)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => FlightSimData.IsInCockpit);

            OpenTouchPanelBindingCommand = new DelegateCommand(OnOpenTouchPanelBinding, () => FlightSimData.HasCurrentMsfsAircraft && FlightSimData.IsSimulatorStarted && ProfileData.HasActiveProfile && AppSettingData.AppSetting.TouchPanelSettings.EnableTouchPanelIntegration)
                                                                                .ObservesProperty(() => FlightSimData.HasCurrentMsfsAircraft)
                                                                                .ObservesProperty(() => ProfileData.HasActiveProfile)
                                                                                .ObservesProperty(() => AppSettingData.AppSetting.TouchPanelSettings.EnableTouchPanelIntegration)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted);

            TouchPanelBindingViewModel = new TouchPanelBindingViewModel(_orchestrator);
        }

        public DelegateCommand AddProfileCommand { get; private set; }

        public DelegateCommand DeleteProfileCommand { get; private set; }

        public DelegateCommand<object> ChangeProfileCommand { get; private set; }

        public DelegateCommand AddProfileBindingCommand { get; private set; }

        public DelegateCommand DeleteProfileBindingCommand { get; private set; }

        public DelegateCommand SetPowerOnRequiredCommand { get; private set; }

        public DelegateCommand StartPanelSelectionCommand { get; private set; }

        public DelegateCommand StartPopOutCommand { get; private set; }

        public DelegateCommand SaveAutoPanningCameraCommand { get; private set; }

        public DelegateCommand EditPanelSourceCommand { get; private set; }

        public DelegateCommand OpenTouchPanelBindingCommand { get; private set; }

        public ProfileData ProfileData { get { return _orchestrator.ProfileData; } }

        public FlightSimData FlightSimData { get { return _orchestrator.FlightSimData; } }

        public AppSettingData AppSettingData { get { return _orchestrator.AppSettingData; } }

        public PanelSourceOrchestrator PanelSource { get { return _orchestrator.PanelSource; } }

        public TouchPanelBindingViewModel TouchPanelBindingViewModel { get; private set; }

        public event EventHandler OpenTouchPanelBindingDialog;

        private void OnAddProfile()
        {
            var result = DialogHelper.AddProfileDialog(ProfileData.Profiles.ToList());

            if (result != null)
                _orchestrator.Profile.AddProfile(result.ProfileName, result.CopyProfileId);
        }

        private void OnDeleteProfile()
        {
            if (DialogHelper.ConfirmDialog("Confirm Delete", "Are you sure you want to delete the selected profile?"))
                _orchestrator.Profile.DeleteActiveProfile();
        }

        private void OnChangeProfile(object commandParameter)
        {
            if (commandParameter == null)
                return;

            var profileId = Convert.ToInt32(commandParameter);
            _orchestrator.Profile.ChangeProfile(profileId);
        }

        private void OnAddProfileBinding()
        {
            _orchestrator.Profile.AddProfileBinding(FlightSimData.CurrentMsfsAircraft);
        }

        private void OnDeleteProfileBinding()
        {
            _orchestrator.Profile.DeleteProfileBinding(FlightSimData.CurrentMsfsAircraft);
        }

        private void OnStartPanelSelection()
        {
            if (!ProfileData.HasActiveProfile)
                return;

            if (ProfileData.ActiveProfile.PanelSourceCoordinates.Count > 0 && !DialogHelper.ConfirmDialog("Confirm Overwrite", "WARNING! Are you sure you want to overwrite existing saved panel locations and all saved setttings for this profile?"))
                return;

            _orchestrator.PanelSource.StartPanelSelection();
        }

        private void OnStartPopOut()
        {
            _orchestrator.PanelPopOut.ManualPopOut();
        }

        private void OnSaveAutoPanningCamera()
        {
            if (!ProfileData.HasActiveProfile)
                return;

            if (ProfileData.ActiveProfile.PanelSourceCoordinates.Count > 0 && !DialogHelper.ConfirmDialog("Confirm Overwrite Auto Panning Camera", "WARNING! Are you sure you want to overwrite existing Auto Panning camera angle?"))
                return;

            _orchestrator.PanelSource.SaveAutoPanningCamera();
        }

        private void EditPanelSource()
        {
            _orchestrator.PanelSource.EditPanelSource();
        }

        private void OnOpenTouchPanelBinding()
        {
            OpenTouchPanelBindingDialog?.Invoke(this, null);
        }
    }
}