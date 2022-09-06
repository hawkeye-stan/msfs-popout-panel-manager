using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.UserDataAgent;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Collections.ObjectModel;
using System.Drawing;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class PanelSourceOrchestrator : ObservableObject
    {
        private int _panelIndex;

        internal ProfileData ProfileData { get; set; }

        internal AppSettingData AppSettingData { get; set; }

        internal FlightSimData FlightSimData { get; set; }

        internal FlightSimOrchestrator FlightSimOrchestrator { get; set; }

        private Profile ActiveProfile { get { return ProfileData == null ? null : ProfileData.ActiveProfile; } }

        private AppSetting AppSetting { get { return AppSettingData == null ? null : AppSettingData.AppSetting; } }

        public event EventHandler<PanelSourceCoordinate> OnOverlayShowed;
        public event EventHandler OnLastOverlayRemoved;
        public event EventHandler OnAllOverlaysRemoved;
        public event EventHandler OnSelectionStarted;
        public event EventHandler OnSelectionCompleted;

        public bool IsEditingPanelSource { get; set; }

        public void StartPanelSelection()
        {
            if (ActiveProfile == null)
                return;

            _panelIndex = 1;

            // remove all existing panel overlay display
            for (var i = 0; i < ActiveProfile.PanelSourceCoordinates.Count; i++)
                OnLastOverlayRemoved?.Invoke(this, null);

            ActiveProfile.PanelSourceCoordinates = new ObservableCollection<PanelSourceCoordinate>();
            ActiveProfile.PanelConfigs.Clear();
            ActiveProfile.IsLocked = false;

            InputHookManager.OnLeftClick -= HandleOnPanelSelectionAdded;
            InputHookManager.OnLeftClick += HandleOnPanelSelectionAdded;
            InputHookManager.OnShiftLeftClick -= HandleOnLastPanelSelectionRemoved;
            InputHookManager.OnShiftLeftClick += HandleOnLastPanelSelectionRemoved;
            InputHookManager.OnCtrlLeftClick -= HandleOnPanelSelectionCompleted;
            InputHookManager.OnCtrlLeftClick += HandleOnPanelSelectionCompleted;

            // Turn off TrackIR if TrackIR is started
            FlightSimOrchestrator.TurnOffTrackIR();

            OnSelectionStarted?.Invoke(this, null);

            InputHookManager.StartHook();
        }

        public void SaveAutoPanningCamera()
        {
            var simualatorProcess = WindowProcessManager.GetSimulatorProcess();
            if (simualatorProcess == null)
            {
                StatusMessageWriter.WriteMessage("MSFS/SimConnect has not been started. Please try again at a later time.", StatusMessageType.Error, false);
                return;
            }

            InputEmulationManager.SaveCustomView(AppSettingData.AppSetting.AutoPanningKeyBinding);

            // If using windows mode, save MSFS game window configuration
            if (AppSettingData.AppSetting.AutoResizeMsfsGameWindow)
                ProfileData.SaveMsfsGameWindowConfig();

            StatusMessageWriter.WriteMessage("Auto Panning Camera has been saved succesfully.", StatusMessageType.Info, false);
        }

        public void EditPanelSource()
        {
            IsEditingPanelSource = !IsEditingPanelSource;

            if (IsEditingPanelSource)
                StartEditPanelSource();
            else
                EndEditPanelSource();
        }

        public void CloseAllPanelSource()
        {
            IsEditingPanelSource = false;
            OnAllOverlaysRemoved?.Invoke(this, null);
        }

        public void HandleOnPanelSelectionAdded(object sender, Point e)
        {
            if (ActiveProfile == null)
                return;

            var newCoor = new PanelSourceCoordinate() { PanelIndex = _panelIndex, X = e.X, Y = e.Y };

            ActiveProfile.PanelSourceCoordinates.Add(newCoor);
            _panelIndex++;

            OnOverlayShowed?.Invoke(this, newCoor);
        }

        public void HandleOnLastPanelSelectionRemoved(object sender, Point e)
        {
            if (ActiveProfile == null)
                return;

            if (ActiveProfile.PanelSourceCoordinates.Count > 0)
            {
                ActiveProfile.PanelSourceCoordinates.RemoveAt(ActiveProfile.PanelSourceCoordinates.Count - 1);
                _panelIndex--;

                OnLastOverlayRemoved?.Invoke(this, null);
            }
        }

        private void StartEditPanelSource()
        {
            if (ActiveProfile == null)
                return;

            // Set Windowed Display Mode window's configuration if needed
            if (AppSettingData.AppSetting.AutoResizeMsfsGameWindow)
                WindowActionManager.SetMsfsGameWindowLocation(ActiveProfile.MsfsGameWindowConfig);

            // remove all existing panel overlay display
            for (var i = 0; i < ActiveProfile.PanelSourceCoordinates.Count; i++)
                OnAllOverlaysRemoved?.Invoke(this, null);

            foreach (var coor in ActiveProfile.PanelSourceCoordinates)
                OnOverlayShowed?.Invoke(this, new PanelSourceCoordinate() { PanelIndex = coor.PanelIndex, X = coor.X, Y = coor.Y });

            // Turn off TrackIR if TrackIR is started
            FlightSimOrchestrator.TurnOffTrackIR();

            if (AppSetting.UseAutoPanning)
                InputEmulationManager.LoadCustomView(AppSetting.AutoPanningKeyBinding);
        }

        private void EndEditPanelSource()
        {
            if (ActiveProfile == null)
                return;

            // Remove all existing panel overlay display
            for (var i = 0; i < ActiveProfile.PanelSourceCoordinates.Count; i++)
                OnAllOverlaysRemoved?.Invoke(this, null);

            // Turn TrackIR back on
            FlightSimOrchestrator.TurnOnTrackIR();
        }

        private void HandleOnPanelSelectionCompleted(object sender, Point e)
        {
            if (ActiveProfile == null)
                return;

            // If enable, save the current viewport into custom view by Ctrl-Alt-0
            if (AppSetting.UseAutoPanning)
                InputEmulationManager.SaveCustomView(AppSetting.AutoPanningKeyBinding);

            ProfileData.WriteProfiles();

            // If using windows mode, save MSFS game window configuration
            if (AppSettingData.AppSetting.AutoResizeMsfsGameWindow)
                ProfileData.SaveMsfsGameWindowConfig();

            InputHookManager.EndHook();

            // Turn TrackIR back on
            FlightSimOrchestrator.TurnOnTrackIR();

            IsEditingPanelSource = false;

            OnSelectionCompleted?.Invoke(this, null);
        }
    }
}
