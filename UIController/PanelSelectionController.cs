using MSFSPopoutPanelManager.Provider;
using MSFSPopoutPanelManager.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager.UIController
{
    public class PanelSelectionController
    {
        private PanelSelectionManager _panelSelectionManager;
        private IPanelSelectionView _view;

        public PanelSelectionController(IPanelSelectionView view, DataStore dataStore) 
        {
            _view = view;
            DataStore = dataStore;

            _panelSelectionManager = new PanelSelectionManager(_view.Form);
            _panelSelectionManager.OnSelectionCompleted += HandleOnPanelSelectionCompleted;
        }

        public DataStore DataStore { get; set; }

        public event EventHandler<EventArgs<PanelSelectionUIState>> OnUIStateChanged;

        public event EventHandler OnPopOutCompleted;

        public void Initialize()
        {
            _view.ShowPanelLocationOverlay = false;

            DataStore.UserProfiles = new BindingList<UserProfileData>(FileManager.ReadUserProfileData().OrderBy(x => x.ProfileName).ToList());
            DataStore.ActiveProfilePanelCoordinates = new BindingList<PanelSourceCoordinate>(DataStore.ActiveUserProfile == null ? new List<PanelSourceCoordinate>() : DataStore.ActiveUserProfile.PanelSourceCoordinates);

            var defaultProfile = FileManager.ReadUserProfileData().Find(x => x.IsDefaultProfile);

            if(defaultProfile == null)
            {
                DataStore.ActiveUserProfileId= -1;
                OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.NoProfileSelected));
            }
            else
            {
                DataStore.ActiveUserProfileId = defaultProfile.ProfileId;
                OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.ProfileSelected));
            }   
        }

        public void AddProfile(string profileName)
        {
            var newProfileId = DataStore.UserProfiles.Count > 0 ? DataStore.UserProfiles.Max(x => x.ProfileId) + 1 : 1;
            var newPlaneProfile = new UserProfileData() { ProfileId = newProfileId, ProfileName = profileName };

            var tmpList = DataStore.UserProfiles.ToList();
            tmpList.Add(newPlaneProfile);
            var index = tmpList.OrderBy(x => x.ProfileName).ToList().FindIndex(x => x.ProfileId == newProfileId);
            DataStore.UserProfiles.Insert(index, newPlaneProfile);
            FileManager.WriteUserProfileData(DataStore.UserProfiles.ToList());  // save the backing list data

            DataStore.ActiveUserProfileId = newProfileId;

            OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.ProfileSelected));
            Logger.Status($"Profile '{newPlaneProfile.ProfileName}' has been added successfully.", StatusMessageType.Info);
        }

        public void DeleteProfile()
        {
            if (DataStore.ActiveUserProfileId != -1)
            {
                var profileToRemove = DataStore.UserProfiles.First(x => x.ProfileId == DataStore.ActiveUserProfileId);
                DataStore.UserProfiles.Remove(profileToRemove);
                FileManager.WriteUserProfileData(DataStore.UserProfiles.ToList());

                _panelSelectionManager.Reset();
                DataStore.ActiveUserProfileId= -1;

                OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.NoProfileSelected));

                Logger.Status($"Profile '{profileToRemove.ProfileName}' has been deleted successfully.", StatusMessageType.Info);
            }
        }

        public void SetDefaultProfile()
        {
            if (DataStore.ActiveUserProfileId== -1)
                return;

            var profile = DataStore.UserProfiles.First(x => x.ProfileId == DataStore.ActiveUserProfileId);

            profile.IsDefaultProfile = true;
            foreach (var p in DataStore.UserProfiles)
            {
                if (p.ProfileId != DataStore.ActiveUserProfileId)
                    p.IsDefaultProfile = false;
            }
            FileManager.WriteUserProfileData(DataStore.UserProfiles.ToList());

            Logger.Status($"Profile '{profile.ProfileName}' has been set as default.", StatusMessageType.Info);
        }

        public void ProfileChanged(int profileId)
        {
            Logger.ClearStatus();

            if (profileId == -1)
            {
                DataStore.ActiveUserProfileId= -1;
                OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.NoProfileSelected));
            }
            else
            {
                DataStore.ActiveUserProfileId = profileId;
                _panelSelectionManager.Reset();
                _view.ShowPanelLocationOverlay = false;
                OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.PanelSelectionCompletedValid));
            }
        }

        public bool HasExistingPanelCoordinates
        {
            get
            {
                return DataStore.ActiveUserProfile != null && DataStore.ActiveUserProfile.PanelSourceCoordinates.Count > 0;
            }
        }

        public void StartPanelSelection()
        {
            DataStore.ActiveUserProfile.Reset();

            _panelSelectionManager.PanelCoordinates = DataStore.ActiveUserProfile.PanelSourceCoordinates;

            OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.PanelSelectionStarted));

            // Temporary minimize the app during panel selection
            if (_view.Form != null)
                _view.Form.WindowState = FormWindowState.Minimized;

            // Set MSFS to foreground for panel selection
            var simulatorProcess = WindowManager.GetSimulatorProcess();
            if (simulatorProcess != null)
                PInvoke.SetForegroundWindow(simulatorProcess.Handle);

            _panelSelectionManager.Start();

            Logger.Status("Panels selection has started.", StatusMessageType.Info);
        }

        private void HandleOnPanelSelectionCompleted(object sender, EventArgs e)
        {
            // If enable, save the current viewport into custom view by Ctrl-Alt-0
            if (FileManager.ReadAppSettingData().UseAutoPanning)
            {
                var simualatorProcess = WindowManager.GetSimulatorProcess();
                if (simualatorProcess != null)
                {
                    InputEmulationManager.SaveCustomViewZero(simualatorProcess.Handle);
                }
            }

            if (_view.Form != null)
                _view.Form.WindowState = FormWindowState.Normal;

            PInvoke.SetForegroundWindow(_view.Form.Handle);

            DataStore.ActiveProfilePanelCoordinates.Clear();
            DataStore.ActiveUserProfile.PanelSourceCoordinates.ForEach(c => DataStore.ActiveProfilePanelCoordinates.Add(c));

            FileManager.WriteUserProfileData(DataStore.UserProfiles.ToList());

            OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.PanelSelectionCompletedValid));

            _view.ShowPanelLocationOverlay = true;

            if (DataStore.ActiveUserProfile.PanelSourceCoordinates.Count > 0)
                Logger.Status("Panels selection is completed. Please click 'Start Pop Out' to start popping out these panels.", StatusMessageType.Info);
            else
                Logger.Status("Panels selection is completed. No panel has been selected.", StatusMessageType.Info);
        }

        public void SetPanelLocationOverlayChanged()
        {
            _panelSelectionManager.PanelCoordinates = DataStore.ActiveProfilePanelCoordinates.ToList();
            _panelSelectionManager.ShowPanelLocationOverlay(_view.ShowPanelLocationOverlay);
        }

        public void StartPopOut()
        {
            if (WindowManager.GetApplicationProcess() == null) 
                return;
            
            OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.PopoutStarted));
            Logger.Status("Panel pop out and separation in progress. Please wait......", StatusMessageType.Info);

            WindowManager.CloseAllCustomPopoutPanels();

            Thread.Sleep(1000);     // allow time for the mouse to be stopped moving by the user

            _panelSelectionManager.ShowPanelLocationOverlay(false);
            _view.ShowPanelLocationOverlay = false;

            var simulatorProcess = WindowManager.GetSimulatorProcess();
            if(simulatorProcess == null)
            {
                Logger.Status("MSFS has not been started. Please try again at a later time.", StatusMessageType.Error);
                OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.PanelSelectionCompletedValid));
                return;
            }

            PopoutSeparationManager popoutSeparationManager = new PopoutSeparationManager(simulatorProcess.Handle, DataStore.ActiveUserProfile);

            // Temporary make app go to background before pop out process
            WindowManager.ApplyAlwaysOnTop(_view.Form.Handle, false, _view.Form.Bounds);

            var result = popoutSeparationManager.StartPopout();

            WindowManager.ApplyAlwaysOnTop(_view.Form.Handle, true, _view.Form.Bounds);

            if (result)
            {
                OnPopOutCompleted?.Invoke(this, null);

                // Save data
                FileManager.WriteUserProfileData(DataStore.UserProfiles.ToList());

                PInvoke.SetForegroundWindow(_view.Form.Handle);
            }
            else
            {
                _panelSelectionManager.ShowPanelLocationOverlay(true);
                _view.ShowPanelLocationOverlay = true;
            }

            OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.PanelSelectionCompletedValid));
        }
    }

    public enum PanelSelectionUIState
    {
        NoProfileSelected,
        ProfileSelected,
        PanelSelectionStarted,
        PanelSelectionCompletedValid,
        PanelSelectionCompletedInvalid,
        PopoutStarted,
    }
}
