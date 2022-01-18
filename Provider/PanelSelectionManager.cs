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
        private Form _appForm;

        public event EventHandler OnSelectionCompleted;

        public List<PanelSourceCoordinate> PanelCoordinates { get; set; }

        public PanelSelectionManager(Form form) 
        {
            PanelCoordinates = new List<PanelSourceCoordinate>();
            _appForm = form;
        }

        public void Start()
        {
            if (_mouseHook == null)
            {
                _mouseHook = Hook.GlobalEvents();
                _mouseHook.MouseDownExt += HandleMouseHookMouseDownExt;
            }

            _panelIndex = 1;
            ShowPanelLocationOverlay(true);
        }

        public void Reset()
        {
            _panelIndex = 1;
            ShowPanelLocationOverlay(false);
        }

        public void ShowPanelLocationOverlay(bool show)
        {
            // close all overlays
            for (int i = Application.OpenForms.Count - 1; i >= 0; i--)
            {
                if (Application.OpenForms[i].GetType() == typeof(PopoutCoorOverlayForm))
                    Application.OpenForms[i].Close();
            }

            if (show && PanelCoordinates.Count > 0)
            {
                foreach (var coor in PanelCoordinates)
                    WindowManager.AddPanelLocationSelectionOverlay(coor.PanelIndex.ToString(), coor.X, coor.Y);
            }
        }

        private void HandleMouseHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var ctrlPressed = Control.ModifierKeys.ToString() == "Control";
                var shiftPressed = Control.ModifierKeys.ToString() == "Shift";

                if (ctrlPressed)
                {
                    if (_mouseHook != null)
                    {
                        _mouseHook.MouseDownExt -= HandleMouseHookMouseDownExt;
                        _mouseHook.Dispose();
                        _mouseHook = null;
                    }

                    OnSelectionCompleted?.Invoke(this, null);
                }
                else if (shiftPressed && Application.OpenForms.Count >= 1)
                {
                    if (Application.OpenForms[Application.OpenForms.Count - 1].GetType() == typeof(PopoutCoorOverlayForm))
                    {
                        // Remove last drawn overlay 
                        Application.OpenForms[Application.OpenForms.Count - 1].Close();
                        PanelCoordinates.RemoveAt(PanelCoordinates.Count - 1);
                        _panelIndex--;
                    }
                }
                else if (!shiftPressed)
                {
                    var minX = _appForm.Location.X;
                    var minY = _appForm.Location.Y;
                    var maxX = _appForm.Location.X + _appForm.Width;
                    var maxY = _appForm.Location.Y + _appForm.Height;

                    if (e.X < minX || e.X > maxX || e.Y < minY || e.Y > maxY)
                    {
                        var newPanelCoordinates = new PanelSourceCoordinate() { PanelIndex = _panelIndex, X = e.X, Y = e.Y };
                        PanelCoordinates.Add(newPanelCoordinates);

                        WindowManager.AddPanelLocationSelectionOverlay(_panelIndex.ToString(), e.X, e.Y);
                        _panelIndex++;
                    }
                }
            }
        }
    }
}