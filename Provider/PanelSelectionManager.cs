using Gma.System.MouseKeyHook;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.UI;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager.Provider
{
    public class PanelSelectionManager
    {
        private IKeyboardMouseEvents _mouseHook;
        private int _panelIndex;

        public event EventHandler OnSelectionStarted;
        public event EventHandler OnSelectionCompleted;
        public event EventHandler<EventArgs<PanelSourceCoordinate>> OnPanelAdded;
        public event EventHandler OnPanelSubtracted;

        public List<PanelSourceCoordinate> PanelCoordinates { get; set; }

        public PanelSelectionManager()
        {
            PanelCoordinates = new List<PanelSourceCoordinate>();
        }

        public Form AppForm { get; set; }

        public void Start()
        {
            if (_mouseHook == null)
            {
                _mouseHook = Hook.GlobalEvents();
                _mouseHook.MouseDownExt += HandleMouseHookMouseDownExt;
            }

            _panelIndex = 1;

            PanelCoordinates = new List<PanelSourceCoordinate>();
            
            ShowPanelLocationOverlay(true);
            OnSelectionStarted?.Invoke(this, null);

            DrawPanelLocationOverlay();

            Logger.Status("Panels selection has started.", StatusMessageType.Info);
        }

        public void Reset()
        {
            PanelCoordinates = new List<PanelSourceCoordinate>();
            _panelIndex = 1;

            DrawPanelLocationOverlay();
        }

        public void DrawPanelLocationOverlay()
        {
            for (int i = Application.OpenForms.Count - 1; i >= 0; i--)
            {
                if (Application.OpenForms[i].GetType() == typeof(PopoutCoorOverlayForm))
                    Application.OpenForms[i].Close();
            }

            if (PanelCoordinates.Count > 0)
            {
                foreach (var coor in PanelCoordinates)
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

        private void Stop()
        {
            if (_mouseHook != null)
            {
                _mouseHook.MouseDownExt -= HandleMouseHookMouseDownExt;
                _mouseHook.Dispose();
                _mouseHook = null;
            }

            // If enable, save the current viewport into custom view by Ctrl-Alt-0
            if (FileManager.ReadAppSettingData().UseAutoPanning)
            {
                var simualatorProcess = WindowManager.GetSimulatorProcess();
                if (simualatorProcess != null)
                {
                    InputEmulationManager.SaveCustomViewZero(simualatorProcess.Handle);
                    Thread.Sleep(500);
                }
            }

            OnSelectionCompleted?.Invoke(this, null);

            DrawPanelLocationOverlay();
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

                    if (PanelCoordinates.Count > 0)
                        Logger.Status("Panels selection is completed. Please click 'Start Pop Out' to start popping out these panels.", StatusMessageType.Info);
                    else
                        Logger.Status("Panels selection is completed. No panel has been selected.", StatusMessageType.Info);

                    // Bring app windows back to top
                    PInvoke.SetForegroundWindow(AppForm.Handle);
                }
                else if (shiftPressed && Application.OpenForms.Count >= 1)
                {
                    // Remove last drawn overlay 
                    Application.OpenForms[Application.OpenForms.Count - 1].Close();
                    PanelCoordinates.RemoveAt(PanelCoordinates.Count - 1);
                    OnPanelSubtracted?.Invoke(this, e);
                    _panelIndex--;

                    DrawPanelLocationOverlay();
                }
                else if (!shiftPressed)
                {
                    var minX = AppForm.Location.X;
                    var minY = AppForm.Location.Y;
                    var maxX = AppForm.Location.X + AppForm.Width;
                    var maxY = AppForm.Location.Y + AppForm.Height;

                    if (e.X < minX || e.X > maxX || e.Y < minY || e.Y > maxY)
                    {
                        var newPanelCoordinates = new PanelSourceCoordinate() { PanelIndex = _panelIndex, X = e.X, Y = e.Y };
                        PanelCoordinates.Add(newPanelCoordinates);
                        OnPanelAdded?.Invoke(this, new EventArgs<PanelSourceCoordinate>(newPanelCoordinates));

                        WindowManager.AddPanelLocationSelectionOverlay(_panelIndex.ToString(), e.X, e.Y);
                        _panelIndex++;
                    }

                    DrawPanelLocationOverlay();
                }
            }
        }
    }
}