using MSFSPopoutPanelManager.Provider;
using MSFSPopoutPanelManager.Shared;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace MSFSPopoutPanelManager.UIController
{
    public class PanelConfigurationController : BaseController
    {
        private const int WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_MOVESIZEEND = 0x000B;

        private static PInvoke.WinEventProc _winEvent;      // keep this as static to prevent garbage collect or the app will crash
        private IntPtr _winEventHook;

        public PanelConfigurationController()
        {
            BaseController.OnPopOutCompleted += HandlePopOutCompleted;
            PanelConfigs = new BindingList<PanelConfig>();
            _winEvent = new PInvoke.WinEventProc(EventCallback);
        }

        public event EventHandler RefreshDataUI;
        public event EventHandler<EventArgs<int>> HightlightSelectedPanel;

        public BindingList<PanelConfig> PanelConfigs { get; set; }

        public void SaveSettings()
        {
            var profile = BaseController.ActiveUserPlaneProfile;
            profile.PanelConfigs = PanelConfigs.ToList();

            var allProfiles = FileManager.ReadUserProfileData();
            var index = allProfiles.FindIndex(x => x.ProfileId == profile.ProfileId);
            allProfiles[index] = profile;
            FileManager.WriteUserProfileData(allProfiles);
        }

        public void BackToPanelSelection()
        {
            // Unhook all Win API events
            PInvoke.UnhookWinEvent(_winEventHook);

            // Try to close all Cutome Panel window
            PanelConfigs.ToList().FindAll(p => p.PanelType == PanelType.CustomPopout).ForEach(panel => WindowManager.CloseWindow(panel.PanelHandle));

            PanelConfigs.Clear();
            Restart();
        }

        public void CellValueChanged(int rowIndex, PanelConfigDataColumn column, object newCellValue)
        {
            if (rowIndex != -1)
            {
                switch (column)
                {
                    case PanelConfigDataColumn.PanelName:
                        PanelConfigs[rowIndex].PanelName = Convert.ToString(newCellValue);
                        PInvoke.SetWindowText(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].PanelName);
                        break;
                    case PanelConfigDataColumn.Left:
                        PanelConfigs[rowIndex].Left = Convert.ToInt32(newCellValue);
                        PInvoke.MoveWindow(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].Left, PanelConfigs[rowIndex].Top, PanelConfigs[rowIndex].Width, PanelConfigs[rowIndex].Height, true);
                        break;
                    case PanelConfigDataColumn.Top:
                        PanelConfigs[rowIndex].Top = Convert.ToInt32(newCellValue);
                        PInvoke.MoveWindow(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].Left, PanelConfigs[rowIndex].Top, PanelConfigs[rowIndex].Width, PanelConfigs[rowIndex].Height, true);
                        break;
                    case PanelConfigDataColumn.Width:
                        PanelConfigs[rowIndex].Width = Convert.ToInt32(newCellValue);
                        PInvoke.MoveWindow(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].Left, PanelConfigs[rowIndex].Top, PanelConfigs[rowIndex].Width, PanelConfigs[rowIndex].Height, true);
                        break;
                    case PanelConfigDataColumn.Height:
                        PanelConfigs[rowIndex].Height = Convert.ToInt32(newCellValue);
                        PInvoke.MoveWindow(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].Left, PanelConfigs[rowIndex].Top, PanelConfigs[rowIndex].Width, PanelConfigs[rowIndex].Height, true);
                        break;
                    case PanelConfigDataColumn.AlwaysOnTop:
                        PanelConfigs[rowIndex].AlwaysOnTop = Convert.ToBoolean(newCellValue);
                        WindowManager.ApplyAlwaysOnTop(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].AlwaysOnTop, new Rectangle(PanelConfigs[rowIndex].Left, PanelConfigs[rowIndex].Top, PanelConfigs[rowIndex].Width, PanelConfigs[rowIndex].Height));
                        break;
                    case PanelConfigDataColumn.HideTitlebar:
                        PanelConfigs[rowIndex].HideTitlebar = Convert.ToBoolean(newCellValue);
                        WindowManager.ApplyHidePanelTitleBar(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].HideTitlebar);
                        break;
                    default:
                        return;
                }
            }
        }

        public void CellValueIncrDecr(int rowIndex, PanelConfigDataColumn column, int changeAmount)
        {
            if (rowIndex != -1)
            {
                switch (column)
                {
                    case PanelConfigDataColumn.Left:
                        PanelConfigs[rowIndex].Left += changeAmount;
                        break;
                    case PanelConfigDataColumn.Top:
                        PanelConfigs[rowIndex].Top += changeAmount;
                        break;
                    case PanelConfigDataColumn.Width:
                        PanelConfigs[rowIndex].Width += changeAmount;
                        break;
                    case PanelConfigDataColumn.Height:
                        PanelConfigs[rowIndex].Height += changeAmount;
                        break;
                    default:
                        return;
                }

                RefreshDataUI?.Invoke(this, null);
                PInvoke.MoveWindow(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].Left, PanelConfigs[rowIndex].Top, PanelConfigs[rowIndex].Width, PanelConfigs[rowIndex].Height, true);
            }
        }

        private void HandlePopOutCompleted(object sender, EventArgs e)
        {
            // Populate panel data
            BaseController.ActiveUserPlaneProfile.PanelConfigs.ForEach(p => PanelConfigs.Add(p));

            // Setup panel config event hooks
            _winEventHook = PInvoke.SetWinEventHook(EVENT_SYSTEM_MOVESIZEEND, EVENT_SYSTEM_MOVESIZEEND, IntPtr.Zero, _winEvent, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        private void EventCallback(IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            var panelConfig = PanelConfigs.FirstOrDefault(panel => panel.PanelHandle == hWnd);

            if (panelConfig != null)
            {
                var rowIndex = PanelConfigs.IndexOf(panelConfig);

                HightlightSelectedPanel?.Invoke(this, new EventArgs<int>(rowIndex));

                if (panelConfig != null)
                {
                    switch (iEvent)
                    {
                        case EVENT_SYSTEM_MOVESIZEEND:
                            Rectangle winRectangle;
                            Rectangle clientRectangle;
                            PInvoke.GetWindowRect(panelConfig.PanelHandle, out winRectangle);
                            PInvoke.GetClientRect(panelConfig.PanelHandle, out clientRectangle);

                            panelConfig.Top = winRectangle.Top;
                            panelConfig.Left = winRectangle.Left;
                            panelConfig.Width = clientRectangle.Width + 16;
                            panelConfig.Height = clientRectangle.Height + 39;
                            break;
                    }

                    RefreshDataUI?.Invoke(this, null);
                }
            }
        }
    }

    public enum PanelConfigDataColumn
    {
        PanelName,
        Left,
        Top,
        Width,
        Height,
        AlwaysOnTop,
        HideTitlebar
    }
}
