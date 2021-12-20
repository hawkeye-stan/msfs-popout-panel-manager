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
    public class PanelSelectionController : BaseController
    {
        private PanelSelectionManager _panelSelectionManager;
        private Form _parentForm;

        public PanelSelectionController() 
        {
            _panelSelectionManager = new PanelSelectionManager();
            _panelSelectionManager.OnSelectionStarted += PanelSelectionManager_OnPanelSelectionStarted;
            _panelSelectionManager.OnSelectionCompleted += PanelSelectionManager_OnPanelSelectionCompleted;
            _panelSelectionManager.OnPanelAdded += PanelSelectionManager_OnPanelAdded;
            _panelSelectionManager.OnPanelSubtracted += PanelSelectionManager_OnPanelSubtracted;

            BaseController.OnRestart += HandleOnRestart;

            PanelCoordinates = new BindingList<PanelSourceCoordinate>();
        }

        public event EventHandler<EventArgs<PanelSelectionUIState>> OnUIStateChanged;

        public int SelectedProfileId { get; set; }

        public UserProfileData ActiveProfile { get { return BaseController.ActiveUserPlaneProfile; } }

        public BindingList<UserProfileData> PlaneProfileList { get; set; }

        public BindingList<PanelSourceCoordinate> PanelCoordinates { get; set; }

        public bool HasActiveProfile { get { return true; } }

        public bool ShowPanelLocationOverlay { get; set; }

        public void Initialize()
        {
            // Setup Defaults
            LoadPlaneProfileList();
            PanelCoordinates.Clear();

            var defaultProfile = FileManager.ReadUserProfileData().Find(x => x.IsDefaultProfile);
            SelectedProfileId = defaultProfile == null ? 0 : defaultProfile.ProfileId;
            ActiveUserPlaneProfile = defaultProfile;
            ShowPanelLocationOverlay = false;

            if (SelectedProfileId < 1)
                OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.NoProfileSelected));
            else
                OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.ProfileSelected));
        }

        public void AddUserProfile(string profileName)
        {
            var profileId = PlaneProfileList.Count > 0 ? PlaneProfileList.Max(x => x.ProfileId) + 1 : 1;

            var newPlaneProfile = new UserProfileData() { ProfileId = profileId, ProfileName = profileName };

            // Find insert index for new profile into list in ascending order
            var list = PlaneProfileList.ToList();
            list.Add(newPlaneProfile);
            var index = list.OrderBy(x => x.ProfileName).ToList().FindIndex(x => x.ProfileId == profileId);
            PlaneProfileList.Insert(index, newPlaneProfile);
            FileManager.WriteUserProfileData(PlaneProfileList.ToList());

            SelectedProfileId = profileId;
            ActiveUserPlaneProfile = newPlaneProfile;

            PanelCoordinates.Clear();

            OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.ProfileSelected));
            Logger.Status($"Profile '{newPlaneProfile.ProfileName}' has been added successfully.", StatusMessageType.Info);
        }

        public void DeleteProfile()
        {
            if (SelectedProfileId != 0)
            {
                var profileToRemove = PlaneProfileList.First(x => x.ProfileId == SelectedProfileId);

                PlaneProfileList.Remove(profileToRemove);
                FileManager.WriteUserProfileData(PlaneProfileList.ToList());

                PanelCoordinates.Clear();
                _panelSelectionManager.Reset();
                SelectedProfileId = -1;

                OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.NoProfileSelected));

                Logger.Status($"Profile '{profileToRemove.ProfileName}' has been deleted successfully.", StatusMessageType.Info);
            }
        }

        public void SetDefaultProfile()
        {
            if (SelectedProfileId < 1)
                return;

            var profile = PlaneProfileList.First(x => x.ProfileId == SelectedProfileId);

            profile.IsDefaultProfile = true;
            foreach (var p in PlaneProfileList)
            {
                if (p.ProfileId != SelectedProfileId)
                    p.IsDefaultProfile = false;
            }
            FileManager.WriteUserProfileData(PlaneProfileList.ToList());

            Logger.Status($"Profile '{profile.ProfileName}' has been set as default.", StatusMessageType.Info);
        }

        public void ProfileChanged(int profileId)
        {
            Logger.ClearStatus();

            PanelCoordinates.Clear();

            SelectedProfileId = profileId;

            if (SelectedProfileId < 1)
            {
                ActiveUserPlaneProfile = null;
                OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.NoProfileSelected));
            }
            else
            {
                ActiveUserPlaneProfile = PlaneProfileList.First(x => x.ProfileId == SelectedProfileId);
                ActiveUserPlaneProfile.PanelSourceCoordinates.ForEach(c => PanelCoordinates.Add(c));

                _panelSelectionManager.PanelCoordinates = PanelCoordinates.ToList();
                _panelSelectionManager.DrawPanelLocationOverlay();
                _panelSelectionManager.ShowPanelLocationOverlay(false);
                ShowPanelLocationOverlay = false;

                //if (PanelCoordinates.Count == 0)
                //    OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.ProfileSelected));
                //else
                OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.PanelSelectionCompletedValid));
            }
        }

        public void StartPanelSelection(Form appForm)
        {
            _parentForm = appForm;

            if (WindowManager.GetApplicationProcess() == null)
                return;
            
            // Temporary make app go to background
            WindowManager.ApplyAlwaysOnTop(_parentForm.Handle, false, _parentForm.Bounds);

            ActiveUserPlaneProfile.PanelConfigs = new List<PanelConfig>();

            _panelSelectionManager.AppForm = appForm;
            _panelSelectionManager.Start();
        }

        public void ShowPanelLocationOverlayChanged(bool show)
        {
            ShowPanelLocationOverlay = show;
            _panelSelectionManager.ShowPanelLocationOverlay(show);
        }

        public void StartPopOut(Form appForm)
        {
            if (WindowManager.GetApplicationProcess() == null) 
                return;
            
            OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.PopoutStarted));
            Logger.Status("Panel pop out and separation in progress. Please wait......", StatusMessageType.Info);
            
            Thread.Sleep(1000);     // allow time for the mouse to be stopped moving by the user

            _panelSelectionManager.ShowPanelLocationOverlay(false);
            ShowPanelLocationOverlay = false;

            var simulatorProcess = WindowManager.GetSimulatorProcess();
            if(simulatorProcess == null)
            {
                Logger.Status("MSFS has not been started. Please try again at a later time.", StatusMessageType.Error);
                OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.PanelSelectionCompletedValid));
                return;
            }

            PopoutSeparationManager popoutSeparationManager = new PopoutSeparationManager(simulatorProcess.Handle, ActiveUserPlaneProfile);

            // Temporary make app go to background before pop out process
            WindowManager.ApplyAlwaysOnTop(appForm.Handle, false, appForm.Bounds);

            var result = popoutSeparationManager.StartPopout();

            WindowManager.ApplyAlwaysOnTop(appForm.Handle, true, appForm.Bounds);

            if (result)
            {
                PopOutCompleted();
                PInvoke.SetForegroundWindow(appForm.Handle);
            }
            else
            {
                _panelSelectionManager.ShowPanelLocationOverlay(true);
                ShowPanelLocationOverlay = true;
            }

            OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.PanelSelectionCompletedValid));
        }

        private void LoadPlaneProfileList()
        {
            if (PlaneProfileList != null)
                PlaneProfileList.Clear();
            else
                PlaneProfileList = new BindingList<UserProfileData>();

            var dataList = FileManager.ReadUserProfileData().OrderBy(x => x.ProfileName);

            foreach (var profile in dataList)
                PlaneProfileList.Add(profile);
        }

        private void PanelSelectionManager_OnPanelSelectionStarted(object sender, EventArgs e)
        {
            PanelCoordinates.Clear();
            OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.PanelSelectionStarted));

            if(_parentForm != null)
                _parentForm.WindowState = FormWindowState.Minimized;

            var simulatorProcess = WindowManager.GetSimulatorProcess();
            if(simulatorProcess != null)
                PInvoke.SetForegroundWindow(simulatorProcess.Handle);
        }

        private void PanelSelectionManager_OnPanelAdded(object sender, EventArgs<PanelSourceCoordinate> e)
        {
            PanelCoordinates.Add(e.Value);
        }

        private void PanelSelectionManager_OnPanelSubtracted(object sender, EventArgs e)
        {
            if (PanelCoordinates.Count > 0)
                PanelCoordinates.RemoveAt(PanelCoordinates.Count - 1);
        }

        private void PanelSelectionManager_OnPanelSelectionCompleted(object sender, EventArgs e)
        {
            if (_parentForm != null)
                _parentForm.WindowState = FormWindowState.Normal;

            //if (PanelCoordinates.Count > 0)
            //{
                ActiveUserPlaneProfile.PanelSourceCoordinates = PanelCoordinates.ToList();
                OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.PanelSelectionCompletedValid));
            //}
            //else
            //    OnUIStateChanged?.Invoke(this, new EventArgs<PanelSelectionUIState>(PanelSelectionUIState.PanelSelectionCompletedInvalid));

            ShowPanelLocationOverlay = true;

            var simulatorProcess = WindowManager.GetSimulatorProcess();
            if (simulatorProcess != null)
                PInvoke.SetForegroundWindow(simulatorProcess.Handle);
        }

        private void HandleOnRestart(object sender, EventArgs e)
        {
            Initialize();
            ProfileChanged(SelectedProfileId);
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
