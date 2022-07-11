using CommunityToolkit.Mvvm.ComponentModel;
using MSFSPopoutPanelManager.Model;
using MSFSPopoutPanelManager.Provider;
using MSFSPopoutPanelManager.Shared;
using System;
using System.Linq;
using System.Threading;
using System.Windows;

namespace MSFSPopoutPanelManager.WpfApp.ViewModel
{
    public class PanelSelectionViewModel : ObservableObject
    {
        private UserProfileManager _userProfileManager;
        private PanelSelectionManager _panelSelectionManager;
        private PanelPopOutManager _panelPopoutManager;
        private SimConnectManager _simConnectManager;
        private bool _minimizeForPopOut;

        public event EventHandler OnPopOutStarted;
        public event EventHandler OnPopOutCompleted;
        public event EventHandler OnUserProfileChanged;

        public DataStore DataStore { get; private set; }

        public bool IsEditingPanelCoorOverlay { get; set; }

        public DelegateCommand AddProfileCommand => new DelegateCommand(OnAddProfile, CanExecute);
        public DelegateCommand DeleteProfileCommand => new DelegateCommand(OnDeleteProfile, CanExecute);
        public DelegateCommand ChangeProfileCommand => new DelegateCommand(OnChangeProfile, CanExecute);
        public DelegateCommand AddProfileBindingCommand => new DelegateCommand(OnAddProfileBinding, CanExecute);
        public DelegateCommand DeleteProfileBindingCommand => new DelegateCommand(OnDeleteProfileBinding, CanExecute);
        public DelegateCommand SetPowerOnRequiredCommand => new DelegateCommand((e) => _userProfileManager.WriteUserProfiles(), CanExecute);
        public DelegateCommand StartPanelSelectionCommand => new DelegateCommand(OnStartPanelSelection, CanExecute);
        public DelegateCommand StartPopOutCommand => new DelegateCommand(OnStartPopOut, CanExecute);
        public DelegateCommand SaveAutoPanningCameraCommand => new DelegateCommand(OnSaveAutoPanningCamera, CanExecute);
        public DelegateCommand EditPanelCoorOverlayCommand => new DelegateCommand(OnEditPanelCoorOverlay, CanExecute);

        public PanelSelectionViewModel(DataStore dataStore, UserProfileManager userProfileManager, PanelPopOutManager panelPopoutManager, SimConnectManager simConnectManager)
        {
            DataStore = dataStore;
            DataStore.OnActiveUserProfileChanged += (sender, e) =>
            {
                if (IsEditingPanelCoorOverlay && DataStore.AppSetting.AutoDisableTrackIR)
                    _simConnectManager.TurnOnTrackIR();

                ShowPanelOverlay(false);
            };

            _userProfileManager = userProfileManager;
            _simConnectManager = simConnectManager;

            _panelSelectionManager = new PanelSelectionManager(userProfileManager);
            _panelSelectionManager.OnPanelLocationAdded += (sender, e) => { AddPanelCoorOverlay(e.Value); };
            _panelSelectionManager.OnPanelLocationRemoved += (sender, e) => { RemoveLastAddedPanelCoorOverlay(); };
            _panelSelectionManager.OnAllPanelLocationsRemoved += (sender, e) => { RemoveAllPanelCoorOverlay(); };
            _panelSelectionManager.OnPanelSelectionCompleted += HandlePanelSelectionCompleted;

            _panelPopoutManager = panelPopoutManager;
            _panelPopoutManager.OnPopOutStarted += HandleOnPopOutStarted;
            _panelPopoutManager.OnPopOutCompleted += HandleOnPopOutCompleted;
        }


        public void Initialize()
        {
            _userProfileManager.ReadUserProfiles();
            DataStore.UserProfiles = _userProfileManager.UserProfiles;
            DataStore.ActiveUserProfileId = DataStore.UserProfiles.ToList().Exists(p => p.ProfileId == DataStore.AppSetting.LastUsedProfileId) ? DataStore.AppSetting.LastUsedProfileId : -1;

            IsEditingPanelCoorOverlay = false;

            InputHookManager.SubscribeToStartPopOutEvent = true;
        }

        private void OnAddProfile(object commandParameter)
        {
            var param = commandParameter as AddProfileCommandParameter;

            if (param.CopyProfileId == -1)
                DataStore.ActiveUserProfileId = _userProfileManager.AddUserProfile(param.ProfileName);
            else
                DataStore.ActiveUserProfileId = _userProfileManager.AddUserProfileByCopyingProfile(param.ProfileName, param.CopyProfileId);

            DataStore.AppSetting.LastUsedProfileId = DataStore.ActiveUserProfileId;
        }

        private void OnDeleteProfile(object commandParameter)
        {
            if (_userProfileManager.DeleteUserProfile(DataStore.ActiveUserProfileId))
                DataStore.ActiveUserProfileId = -1;
        }

        private void OnChangeProfile(object commandParameter)
        {
            DataStore.AppSetting.LastUsedProfileId = DataStore.ActiveUserProfileId;
            OnUserProfileChanged?.Invoke(this, null);
        }

        private void OnAddProfileBinding(object commandParameter)
        {
            var index = DataStore.ActiveUserProfileId;

            _userProfileManager.AddProfileBinding(DataStore.CurrentMsfsPlaneTitle, DataStore.ActiveUserProfileId);

            // force profile refresh
            DataStore.ActiveUserProfileId = -1;
            DataStore.ActiveUserProfileId = index;
        }

        private void OnDeleteProfileBinding(object commandParameter)
        {
            var index = DataStore.ActiveUserProfileId;

            _userProfileManager.DeleteProfileBinding(DataStore.CurrentMsfsPlaneTitle, DataStore.ActiveUserProfileId);

            // force profile refresh
            DataStore.ActiveUserProfileId = -1;
            DataStore.ActiveUserProfileId = index;
        }

        private void OnStartPanelSelection(object commandParameter)
        {
            // Turn off TrackIR if TrackIR is started
            if (DataStore.AppSetting.AutoDisableTrackIR)
                _simConnectManager.TurnOffTrackIR();

            RemoveAllPanelCoorOverlay();

            WindowManager.MinimizeWindow(DataStore.ApplicationHandle);      // Window hide doesn't work when try to reshow window after selection completes. So need to use minimize.
            _panelSelectionManager.UserProfile = DataStore.ActiveUserProfile;
            _panelSelectionManager.AppSetting = DataStore.AppSetting;
            _panelSelectionManager.Start();
        }

        private void OnStartPopOut(object commandParameter)
        {
            Thread.Sleep(500);     // allow time for the mouse to be stopped moving by the user

            ShowPanelOverlay(false);
            InputHookManager.EndHook();

            if (!(DataStore.IsSimulatorStarted))
            {
                Logger.LogStatus("MSFS/SimConnect has not been started. Please try again at a later time.", StatusMessageType.Error);
                return;
            }

            if(DataStore.ActiveUserProfile.PanelSourceCoordinates.Count == 0)
            {
                Logger.LogStatus("No panel has been selected for the profile. Please select at least one panel.", StatusMessageType.Error);
                return;
            }

            if (DataStore.ActiveUserProfile != null && DataStore.ActiveUserProfile.PanelSourceCoordinates.Count > 0)
            {
                // Turn off TrackIR if TrackIR is started
                if (DataStore.AppSetting.AutoDisableTrackIR)
                    _simConnectManager.TurnOffTrackIR();

                Logger.LogStatus("Panels pop out in progress.....", StatusMessageType.Info);

                var messageDialog = new OnScreenMessageDialog($"Panels pop out in progress for profile:\n{DataStore.ActiveUserProfile.ProfileName}", DataStore.AppSetting.AutoPopOutPanelsWaitDelay.InitialCockpitView);
                messageDialog.ShowDialog();

                // Turn on power if required to pop out panels
                _simConnectManager.TurnOnPower(DataStore.ActiveUserProfile.PowerOnRequiredForColdStart);
                Thread.Sleep(DataStore.AppSetting.AutoPopOutPanelsWaitDelay.InstrumentationPowerOn * 1000);     // Wait for battery to be turned on

                _panelPopoutManager.UserProfile = DataStore.ActiveUserProfile;
                _panelPopoutManager.AppSetting = DataStore.AppSetting;
                _panelPopoutManager.StartPopout();

                // Turn off power if needed after pop out
                _simConnectManager.TurnOffpower();
            }
        }

        private void OnSaveAutoPanningCamera(object commandParameter)
        {
            var simualatorProcess = DiagnosticManager.GetSimulatorProcess();
            if (simualatorProcess == null)
            {
                Logger.LogStatus("MSFS/SimConnect has not been started. Please try again at a later time.", StatusMessageType.Error);
                return;
            }

            InputEmulationManager.SaveCustomView(simualatorProcess.Handle, DataStore.AppSetting.AutoPanningKeyBinding);
            Logger.LogStatus("Auto Panning Camera has been saved succesfully.", StatusMessageType.Info);
        }

        private void OnEditPanelCoorOverlay(object commandParameter)
        {
            // Turn off TrackIR if TrackIR is started
            if (DataStore.AppSetting.AutoDisableTrackIR)
            {
                if (IsEditingPanelCoorOverlay)
                    _simConnectManager.TurnOffTrackIR();
                else
                    _simConnectManager.TurnOnTrackIR();
            }

            if (IsEditingPanelCoorOverlay)
            {
                ShowPanelOverlay(true);
                _panelSelectionManager.UserProfile = DataStore.ActiveUserProfile;
                _panelSelectionManager.AppSetting = DataStore.AppSetting;

                //if (DataStore.AppSetting.UseAutoPanning)
                //    InputEmulationManager.LoadCustomView(DataStore.AppSetting.AutoPanningKeyBinding);

                InputHookManager.StartHook();
            }
            else
            {
                InputHookManager.EndHook();
                ShowPanelOverlay(false);
            }
        }

        private void AddPanelCoorOverlay(PanelSourceCoordinate panelSourceCoordinate)
        {
            PanelCoorOverlay overlay = new PanelCoorOverlay(panelSourceCoordinate.PanelIndex);
            overlay.IsEditingPanelLocation = IsEditingPanelCoorOverlay;
            overlay.WindowStartupLocation = WindowStartupLocation.Manual;
            overlay.MoveWindow(panelSourceCoordinate.X, panelSourceCoordinate.Y);
            overlay.ShowInTaskbar = false;
            overlay.Show();
            overlay.WindowLocationChanged += (sender, e) =>
            {
                panelSourceCoordinate.X = e.Value.X;
                panelSourceCoordinate.Y = e.Value.Y;
                _userProfileManager.WriteUserProfiles();
            };
        }

        private void RemoveLastAddedPanelCoorOverlay()
        {
            RemovePanelCoorOverlay(false);
        }

        private void RemoveAllPanelCoorOverlay()
        {
            RemovePanelCoorOverlay(true);
        }

        private void RemovePanelCoorOverlay(bool removeAll)
        {
            for (int i = Application.Current.Windows.Count - 1; i >= 1; i--)
            {
                if (Application.Current.Windows[i].GetType() == typeof(PanelCoorOverlay))
                {
                    Application.Current.Windows[i].Close();
                    if (!removeAll)
                        break;
                }
            }
        }

        private void HandleOnPopOutStarted(object sender, EventArgs e)
        {
            // Hide panel coordinate overlays
            ShowPanelOverlay(false);
            
            // Close all pop out panels
            WindowManager.CloseAllCustomPopoutPanels();

            // Turn off TrackIR if TrackIR is started
            if (DataStore.AppSetting.AutoDisableTrackIR)
                _simConnectManager.TurnOffTrackIR();

            // Temporary minimize the app for pop out process
            _minimizeForPopOut = DataStore.ApplicationWindow.WindowState != WindowState.Minimized;
            if (_minimizeForPopOut)
                WindowManager.MinimizeWindow(DataStore.ApplicationHandle);

            OnPopOutStarted?.Invoke(this, null);
        }

        private void HandleOnPopOutCompleted(object sender, EventArgs<bool> hasResult)
        {
            // Restore window state
            if (_minimizeForPopOut)
            {
                WindowManager.BringWindowToForeground(DataStore.ApplicationHandle);
                DataStore.ApplicationWindow.Show();
            }

            if (hasResult.Value)
            {
                OnPopOutCompleted?.Invoke(this, null);
            }

            if (DataStore.AppSetting.AutoDisableTrackIR)
                _simConnectManager.TurnOnTrackIR();
        }

        private void HandlePanelSelectionCompleted(object sender, EventArgs e)
        {
            WindowManager.BringWindowToForeground(DataStore.ApplicationHandle);
            DataStore.ApplicationWindow.Show();

            if (DataStore.ActiveUserProfile.PanelSourceCoordinates.Count > 0)
                Logger.LogStatus("Panels selection is completed. Please click 'Start Pop Out' to start popping out these panels.", StatusMessageType.Info);
            else
                Logger.LogStatus("Panels selection is completed. No panel has been selected.", StatusMessageType.Info);

            IsEditingPanelCoorOverlay = true;

            ShowPanelOverlay(true);
        }

        private bool CanExecute(object commandParameter)
        {
            return true;
        }

        private void ShowPanelOverlay(bool show)
        {
            IsEditingPanelCoorOverlay = show;
            RemoveAllPanelCoorOverlay();

            if (show && DataStore.ActiveUserProfile != null)
                DataStore.ActiveUserProfile.PanelSourceCoordinates.ToList().ForEach(c => AddPanelCoorOverlay(c));
        }
    }

    public class AddProfileCommandParameter
    {
        public string ProfileName { get; set; }

        public int CopyProfileId { get; set; }
    }
}