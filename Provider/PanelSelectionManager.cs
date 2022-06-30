using MSFSPopoutPanelManager.Model;
using MSFSPopoutPanelManager.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MSFSPopoutPanelManager.Provider
{
    public class PanelSelectionManager
    {
        private UserProfileManager _userProfileManager;
        private int _panelIndex;
        private List<PanelSourceCoordinate> _panelCoordinates;
        private IntPtr _winEventHook;
        private static PInvoke.WinEventProc _winEvent;      // keep this as static to prevent garbage collect or the app will crash
        private Rectangle _lastWindowRectangle;
        private bool _isEditingPanelCoordinates;

        public event EventHandler OnPanelSelectionCompleted;
        public event EventHandler<EventArgs<PanelSourceCoordinate>> OnPanelLocationAdded;
        public event EventHandler OnPanelLocationRemoved;
        public event EventHandler OnAllPanelLocationsRemoved;

        public UserProfile UserProfile { get; set; }

        public AppSetting AppSetting { get; set; }

        public PanelSelectionManager(UserProfileManager userProfileManager)
        {
            _userProfileManager = userProfileManager;

            InputHookManager.OnPanelSelectionAdded += HandleOnPanelSelectionAdded;
            InputHookManager.OnPanelSelectionRemoved += HandleOnPanelSelectionRemoved;
            InputHookManager.OnPanelSelectionCompleted += HandleOnPanelSelectionCompleted;
        }

        public void Start()
        {
            _panelIndex = 1;
            _panelCoordinates = new List<PanelSourceCoordinate>();
            ShowPanelLocationOverlay(_panelCoordinates, true);
            InputHookManager.SubscribeToPanelSelectionEvent = true;
        }

        public void ShowPanelLocationOverlay(List<PanelSourceCoordinate> panelCoordinates, bool show)
        {
            // close all overlays
            OnAllPanelLocationsRemoved?.Invoke(this, null);

            if (show && panelCoordinates.Count > 0)
            {
                foreach (var coor in panelCoordinates)
                {
                    var panelSourceCoordinate = new PanelSourceCoordinate() { PanelIndex = coor.PanelIndex, X = coor.X, Y = coor.Y };
                    OnPanelLocationAdded?.Invoke(this, new EventArgs<PanelSourceCoordinate>(panelSourceCoordinate));
                }
            }
        }

        private void HandleOnPanelSelectionAdded(object sender, System.Drawing.Point e)
        {
            var newPanelCoordinates = new PanelSourceCoordinate() { PanelIndex = _panelIndex, X = e.X, Y = e.Y };
            _panelCoordinates.Add(newPanelCoordinates);
            _panelIndex++;

            OnPanelLocationAdded?.Invoke(this, new EventArgs<PanelSourceCoordinate>(newPanelCoordinates));
        }

        private void HandleOnPanelSelectionRemoved(object sender, EventArgs e)
        {
            if (_panelCoordinates.Count > 0)
            {
                _panelCoordinates.RemoveAt(_panelCoordinates.Count - 1);
                _panelIndex--;

                OnPanelLocationRemoved?.Invoke(this, null);
            }
        }

        private void HandleOnPanelSelectionCompleted(object sender, EventArgs e)
        {
            // If enable, save the current viewport into custom view by Ctrl-Alt-0
            if (AppSetting.UseAutoPanning)
            {
                var simualatorProcess = DiagnosticManager.GetSimulatorProcess();
                if (simualatorProcess != null)
                {
                    InputEmulationManager.SaveCustomView(simualatorProcess.Handle, AppSetting.AutoPanningKeyBinding);
                }
            }

            // Assign and save panel coordinates to active profile
            UserProfile.PanelSourceCoordinates.Clear();
            _panelCoordinates.ForEach(c => UserProfile.PanelSourceCoordinates.Add(c));
            UserProfile.PanelConfigs.Clear();
            UserProfile.IsLocked = false;

            _userProfileManager.WriteUserProfiles();

            InputHookManager.SubscribeToPanelSelectionEvent = false;
            OnPanelSelectionCompleted?.Invoke(this, null);
        }

        public void StartEditPanelLocations()
        {
            _winEvent = new PInvoke.WinEventProc(PanelLocationEditEventCallback);
            _winEventHook = PInvoke.SetWinEventHook(PInvokeConstant.EVENT_SYSTEM_MOVESIZEEND, PInvokeConstant.EVENT_OBJECT_LOCATIONCHANGE, DiagnosticManager.GetApplicationProcess().Handle, _winEvent, 0, 0, PInvokeConstant.WINEVENT_OUTOFCONTEXT);
            _isEditingPanelCoordinates = true;
        }

        public void EndEditPanelLocations()
        {
            PInvoke.UnhookWinEvent(_winEventHook);
            _isEditingPanelCoordinates = false;
        }

        private void PanelLocationEditEventCallback(IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            // check by priority to minimize escaping constraint
            if (hWnd == IntPtr.Zero
                || idObject != 0
                || hWinEventHook != _winEventHook
                || !_isEditingPanelCoordinates
                || !(iEvent == PInvokeConstant.EVENT_OBJECT_LOCATIONCHANGE || iEvent == PInvokeConstant.EVENT_SYSTEM_MOVESIZEEND)
                || UserProfile.PanelSourceCoordinates == null || UserProfile.PanelSourceCoordinates.Count == 0)
            {
                return;
            }

            var panelSourceCoordinate = UserProfile.PanelSourceCoordinates.FirstOrDefault(panel => panel.PanelHandle == hWnd);

            if (panelSourceCoordinate != null)
            {
                switch (iEvent)
                {
                    case PInvokeConstant.EVENT_OBJECT_LOCATIONCHANGE:
                        Rectangle winRectangle;
                        PInvoke.GetWindowRect(panelSourceCoordinate.PanelHandle, out winRectangle);

                        if (_lastWindowRectangle == winRectangle)       // ignore duplicate callback messages
                            return;

                        _lastWindowRectangle = winRectangle;
                        Rectangle clientRectangle;
                        PInvoke.GetClientRect(panelSourceCoordinate.PanelHandle, out clientRectangle);

                        panelSourceCoordinate.X = winRectangle.Left;
                        panelSourceCoordinate.Y = winRectangle.Top;

                        break;
                    case PInvokeConstant.EVENT_SYSTEM_MOVESIZEEND:
                        _userProfileManager.WriteUserProfiles();
                        break;
                }
            }
        }
    }
}
