using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager
{
    public class PanelLocationSelectionModule
    {
        private IKeyboardMouseEvents _mouseHook;
        private int _panelIndex;
        private Form _appForm;

        public event EventHandler OnLocationListChanged;
        public event EventHandler OnSelectionStarted;
        public event EventHandler OnSelectionCompleted;

        public PanelLocationSelectionModule(Form appForm)
        {
            _appForm = appForm;
        }

        public UserPlaneProfile PlaneProfile { get; set; }

        public void Start()
        {
            if (_mouseHook == null)
            {
                _mouseHook = Hook.GlobalEvents();
                _mouseHook.MouseDownExt += HandleMouseHookMouseDownExt;
            }

            _panelIndex = 1;

            PlaneProfile.PanelSourceCoordinates = new List<PanelSourceCoordinate>();
            PlaneProfile.PanelSettings = new PanelSettings();
            ShowPanelLocationOverlay(true);
            OnSelectionStarted?.Invoke(this, null);

            UpdatePanelLocationUI();

            Logger.LogStatus("Panels selection has started.");
        }

        public void Reset()
        {
            PlaneProfile.PanelSourceCoordinates = new List<PanelSourceCoordinate>();
            _panelIndex = 1;

            UpdatePanelLocationUI();
        }

        public void DrawPanelLocationOverlay()
        {
            for (int i = Application.OpenForms.Count - 1; i >= 0; i--)
            {
                if (Application.OpenForms[i].GetType() == typeof(PopoutCoorOverlayForm))
                    Application.OpenForms[i].Close();
            }

            if (PlaneProfile.PanelSourceCoordinates != null && PlaneProfile.PanelSourceCoordinates.Count > 0)
            {
                foreach (var coor in PlaneProfile.PanelSourceCoordinates)
                    WindowManager.AddPanelLocationSelectionOverlay(coor.PanelIndex.ToString(), coor.X, coor.Y);
            }
        }

        public void ShowPanelLocationOverlay(bool show)
        {
            for (int i = 0; i < Application.OpenForms.Count; i++)
            {
                if (Application.OpenForms[i].GetType() == typeof(PopoutCoorOverlayForm))
                    Application.OpenForms[i].Visible = show;
            }
        }

        public List<PanelSourceCoordinate> PanelCoordinates
        {
            get 
            {
                return PlaneProfile.PanelSourceCoordinates;
            }
        }

        private void Stop()
        {
            if (_mouseHook != null)
            {
                _mouseHook.MouseDownExt -= HandleMouseHookMouseDownExt;
                _mouseHook.Dispose();
                _mouseHook = null;
            }

            OnSelectionCompleted?.Invoke(this, null);

            UpdatePanelLocationUI();
        }

        private void HandleMouseHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var ctrlPressed = Control.ModifierKeys.ToString() == "Control";
                var shiftPressed = Control.ModifierKeys.ToString() == "Shift";

                if (ctrlPressed)
                {
                    Stop();

                    if (PlaneProfile.PanelSourceCoordinates.Count > 0)
                        Logger.LogStatus($"Panels selection completed and {PlaneProfile.PanelSourceCoordinates.Count} panel(s) have been selected. Please click 'Analyze' to start popping out these panels.");
                    else
                        Logger.LogStatus("Panels selection completed. No panel has been selected.");

                    // Bring app windows back to top
                    PInvoke.SetForegroundWindow(_appForm.Handle);
                }
                else if (shiftPressed && Application.OpenForms.Count > 1)
                {
                    // Remove last drawn overlay and last value
                    Application.OpenForms[Application.OpenForms.Count - 1].Close();
                    PlaneProfile.PanelSourceCoordinates.RemoveAt(PlaneProfile.PanelSourceCoordinates.Count - 1);
                    _panelIndex--;

                    UpdatePanelLocationUI();
                }
                else if (!shiftPressed)
                {
                    var minX = _appForm.Location.X;
                    var minY = _appForm.Location.Y;
                    var maxX = _appForm.Location.X + _appForm.Width;
                    var maxY = _appForm.Location.Y + _appForm.Height;

                    if (e.X < minX || e.X > maxX || e.Y < minY || e.Y > maxY)
                    {
                        PlaneProfile.PanelSourceCoordinates.Add(new PanelSourceCoordinate() { PanelIndex = _panelIndex, X = e.X, Y = e.Y });

                        WindowManager.AddPanelLocationSelectionOverlay(_panelIndex.ToString(), e.X, e.Y);
                        _panelIndex++;
                    }

                    UpdatePanelLocationUI();
                }
            }
        }

        public void UpdatePanelLocationUI()
        {
            OnLocationListChanged?.Invoke(this, null);
            DrawPanelLocationOverlay();
        }
    }
}
