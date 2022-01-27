using MSFSPopoutPanelManager.Model;
using MSFSPopoutPanelManager.Provider;
using MSFSPopoutPanelManager.Shared;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace MSFSPopoutPanelManager.WpfApp.ViewModel
{
    public class PanelSelectionViewModel : INotifyPropertyChanged
    {
        private UserProfileManager _userProfileManager;
        private PanelSelectionManager _panelSelectionManager;
        private PanelPopOutManager _panelPopoutManager;
        private SimConnectManager _simConnectManager;
        private bool _minimizeForPopOut;

        // Using PropertyChanged.Fody
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler OnPopOutStarted;
        public event EventHandler OnPopOutCompleted;
        public event EventHandler OnShowPrePopOutMessage;

        public DataStore DataStore { get; set; }

        public bool IsEditingPanelCoorOverlay { get; set; }

        public DelegateCommand AddProfileCommand => new DelegateCommand(OnAddProfile, CanExecute);
        public DelegateCommand DeleteProfileCommand => new DelegateCommand(OnDeleteProfile, CanExecute);
        public DelegateCommand SetDefaultProfileCommand => new DelegateCommand(OnSetDefaultProfile, CanExecute);
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
            DataStore.OnActiveUserProfileChanged += (sender, e) => { IsEditingPanelCoorOverlay = false; RemoveAllPanelCoorOverlay(); };

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
            
            var defaultProfile = _userProfileManager.GetDefaultProfile();
            DataStore.ActiveUserProfileId = defaultProfile == null ? -1 : defaultProfile.ProfileId;

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
        }

        private void OnDeleteProfile(object commandParameter)
        {
            if (_userProfileManager.DeleteUserProfile(DataStore.ActiveUserProfileId))
                DataStore.ActiveUserProfileId = -1;
        }

        private void OnSetDefaultProfile(object commandParameter)
        {
            _userProfileManager.SetDefaultUserProfile(DataStore.ActiveUserProfileId);
        }

        private void OnAddProfileBinding(object commandParameter)
        {
            _userProfileManager.AddProfileBinding(DataStore.CurrentMsfsPlaneTitle, DataStore.ActiveUserProfileId);
        }

        private void OnDeleteProfileBinding(object commandParameter)
        {
            _userProfileManager.DeleteProfileBinding(DataStore.ActiveUserProfileId);
        }

        private void OnStartPanelSelection(object commandParameter)
        {
            WindowManager.MinimizeWindow(DataStore.ApplicationHandle);      // Window hide doesn't work when try to reshow window after selection completes. So need to use minimize.
            _panelSelectionManager.UserProfile = DataStore.ActiveUserProfile;
            _panelSelectionManager.AppSetting = DataStore.AppSetting;
            _panelSelectionManager.Start();
        }

        private void OnStartPopOut(object commandParameter)
        {
            Thread.Sleep(500);     // allow time for the mouse to be stopped moving by the user

            var simulatorProcess = DiagnosticManager.GetSimulatorProcess();

            if (!(DataStore.IsSimulatorStarted && DataStore.IsFlightActive))
            {
                Logger.LogStatus("MSFS/SimConnect has not been started. Please try again at a later time.", StatusMessageType.Error);
                return;
            }

            if (DataStore.ActiveUserProfile.PanelSourceCoordinates.Count > 0)
            {
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
            if (simualatorProcess != null && DataStore.IsFlightActive)
            {
                InputEmulationManager.SaveCustomViewZero(simualatorProcess.Handle);
                Logger.LogStatus("Auto Panning Camera has been saved succesfully.", StatusMessageType.Info);
            }
        }

        private void OnEditPanelCoorOverlay(object commandParameter)
        {
            if (IsEditingPanelCoorOverlay)
            {
                RemoveAllPanelCoorOverlay();
                DataStore.ActiveUserProfile.PanelSourceCoordinates.ToList().ForEach(c => AddPanelCoorOverlay(c));

                _panelSelectionManager.UserProfile = DataStore.ActiveUserProfile;
                _panelSelectionManager.AppSetting = DataStore.AppSetting;
                _panelSelectionManager.StartEditPanelLocations();
            }
            else
            {
                _panelSelectionManager.EndEditPanelLocations();
                RemoveAllPanelCoorOverlay();
            }
        }

        private void AddPanelCoorOverlay(PanelSourceCoordinate panelSourceCoordinate)
        {
            PanelCoorOverlay overlay = new PanelCoorOverlay(panelSourceCoordinate.PanelIndex);
            overlay.IsEditingPanelLocation = IsEditingPanelCoorOverlay;
            overlay.Loaded += (sender, e) =>
            {
                var overlay = (Window)sender;
                var handle = new WindowInteropHelper(Window.GetWindow(overlay)).Handle;
                panelSourceCoordinate.PanelHandle = handle;
                PInvoke.MoveWindow(handle, (int)overlay.Left, (int)overlay.Top, (int)overlay.Width, (int)overlay.Height, false);
            };
            overlay.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            overlay.Left = panelSourceCoordinate.X - overlay.Width / 2;
            overlay.Top = panelSourceCoordinate.Y - overlay.Height / 2;
            overlay.ShowInTaskbar = false;
            overlay.Show();
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
            IsEditingPanelCoorOverlay = false;
            OnEditPanelCoorOverlay(null);

            // Close all pop out panels
            WindowManager.CloseAllCustomPopoutPanels();

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
        }

        private void HandlePanelSelectionCompleted(object sender, EventArgs e)
        {
            WindowManager.BringWindowToForeground(DataStore.ApplicationHandle);
            DataStore.ApplicationWindow.Show();

            IsEditingPanelCoorOverlay = true;
            OnEditPanelCoorOverlay(null);

            if (DataStore.ActiveUserProfile.PanelSourceCoordinates.Count > 0)
                Logger.LogStatus("Panels selection is completed. Please click 'Start Pop Out' to start popping out these panels.", StatusMessageType.Info);
            else
                Logger.LogStatus("Panels selection is completed. No panel has been selected.", StatusMessageType.Info);
        }

        private bool CanExecute(object commandParameter)
        {
            return true;
        }
    }

    public class AddProfileCommandParameter
    {
        public string ProfileName { get; set; }

        public int CopyProfileId { get; set; }
    }
}