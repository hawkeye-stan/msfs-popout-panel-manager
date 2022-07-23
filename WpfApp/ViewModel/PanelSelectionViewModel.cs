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

            AddProfileCommand = new DelegateCommand(OnAddProfile);

            DeleteProfileCommand = new DelegateCommand(OnDeleteProfile, () => ProfileData.HasActiveProfile)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile);

            ChangeProfileCommand = new DelegateCommand<object>(OnChangeProfile);

            AddProfileBindingCommand = new DelegateCommand(OnAddProfileBinding, () => ProfileData.HasActiveProfile && FlightSimData.HasCurrentMsfsPlaneTitle && ProfileData.IsAllowedAddAircraftBinding && FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => FlightSimData.HasCurrentMsfsPlaneTitle)
                                                                                .ObservesProperty(() => ProfileData.HasActiveProfile)
                                                                                .ObservesProperty(() => ProfileData.IsAllowedAddAircraftBinding)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted);

            DeleteProfileBindingCommand = new DelegateCommand(OnDeleteProfileBinding, () => ProfileData.HasActiveProfile && ProfileData.IsAllowedDeleteAircraftBinding && ProfileData.ActiveProfile.BindingAircraftLiveries.Count > 0 && FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => ProfileData.HasActiveProfile)
                                                                                .ObservesProperty(() => ProfileData.IsAllowedDeleteAircraftBinding)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.BindingAircraftLiveries.Count)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted);

            SetPowerOnRequiredCommand = new DelegateCommand(() => ProfileData.WriteProfiles(), () => ProfileData.HasActiveProfile && FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted);

            StartPanelSelectionCommand = new DelegateCommand(OnStartPanelSelection, () => ProfileData.HasActiveProfile && ProfileData.ActiveProfile != null && FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted);

            StartPopOutCommand = new DelegateCommand(OnStartPopOut, () => ProfileData.HasActiveProfile && (ProfileData.ActiveProfile.PanelSourceCoordinates.Count > 0 || ProfileData.ActiveProfile.TouchPanelBindings.Count > 0) && FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.PanelSourceCoordinates.Count)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.TouchPanelBindings.Count)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted);

            SaveAutoPanningCameraCommand = new DelegateCommand(OnSaveAutoPanningCamera, () => ProfileData.HasActiveProfile && ProfileData.ActiveProfile.PanelSourceCoordinates.Count > 0 && FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.PanelSourceCoordinates.Count)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted);

            EditPanelSourceCommand = new DelegateCommand(EditPanelSource, () => ProfileData.HasActiveProfile && ProfileData.ActiveProfile.PanelSourceCoordinates.Count > 0 && FlightSimData.IsSimulatorStarted)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.PanelSourceCoordinates.Count)
                                                                                .ObservesProperty(() => FlightSimData.IsSimulatorStarted);

            OpenTouchPanelBindingCommand = new DelegateCommand(OnOpenTouchPanelBinding, () => ProfileData.HasActiveProfile && AppSettingData.AppSetting.TouchPanelSettings.EnableTouchPanelIntegration)
                                                                    .ObservesProperty(() => ProfileData.HasActiveProfile)
                                                                    .ObservesProperty(() => AppSettingData.AppSetting.TouchPanelSettings.EnableTouchPanelIntegration);

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
            _orchestrator.Profile.AddProfileBinding(FlightSimData.CurrentMsfsPlaneTitle);
        }

        private void OnDeleteProfileBinding()
        {
            _orchestrator.Profile.DeleteProfileBinding(FlightSimData.CurrentMsfsPlaneTitle);
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