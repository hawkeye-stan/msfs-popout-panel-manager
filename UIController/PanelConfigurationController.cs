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
        //private const uint EVENT_SYSTEM_MOVESIZEEND = 0x000B;
        private const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B;

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
            int orignalLeft = PanelConfigs[rowIndex].Left;

            if (rowIndex != -1)
            {
                switch (column)
                {
                    case PanelConfigDataColumn.PanelName:
                        PInvoke.SetWindowText(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].PanelName);
                        break;
                    case PanelConfigDataColumn.Left:
                        PInvoke.MoveWindow(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].Left, PanelConfigs[rowIndex].Top, PanelConfigs[rowIndex].Width, PanelConfigs[rowIndex].Height, true);
                        break;
                    case PanelConfigDataColumn.Top:
                        PInvoke.MoveWindow(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].Left, PanelConfigs[rowIndex].Top, PanelConfigs[rowIndex].Width, PanelConfigs[rowIndex].Height, true);
                        break;
                    case PanelConfigDataColumn.Width:
                        PInvoke.MoveWindow(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].Left, PanelConfigs[rowIndex].Top, PanelConfigs[rowIndex].Width, PanelConfigs[rowIndex].Height, true);
                        MSFSBugPanelShiftWorkaround(PanelConfigs[rowIndex].PanelHandle, orignalLeft, PanelConfigs[rowIndex].Top, PanelConfigs[rowIndex].Width, PanelConfigs[rowIndex].Height);
                        break;
                    case PanelConfigDataColumn.Height:
                        PInvoke.MoveWindow(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].Left, PanelConfigs[rowIndex].Top, PanelConfigs[rowIndex].Width, PanelConfigs[rowIndex].Height, true);
                        MSFSBugPanelShiftWorkaround(PanelConfigs[rowIndex].PanelHandle, orignalLeft, PanelConfigs[rowIndex].Top, PanelConfigs[rowIndex].Width, PanelConfigs[rowIndex].Height);
                        break;
                    case PanelConfigDataColumn.AlwaysOnTop:
                        WindowManager.ApplyAlwaysOnTop(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].AlwaysOnTop, new Rectangle(PanelConfigs[rowIndex].Left, PanelConfigs[rowIndex].Top, PanelConfigs[rowIndex].Width, PanelConfigs[rowIndex].Height));
                        break;
                    case PanelConfigDataColumn.HideTitlebar:
                        WindowManager.ApplyHidePanelTitleBar(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].HideTitlebar);
                        break;
                    default:
                        return;
                }
            }
        }

        public void CellValueIncrDecr(int rowIndex, PanelConfigDataColumn column, int changeAmount)
        {
            int orignalLeft = PanelConfigs[rowIndex].Left;

            if (rowIndex != -1)
            {
                switch (column)
                {
                    case PanelConfigDataColumn.Left:
                        PInvoke.MoveWindow(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].Left + changeAmount, PanelConfigs[rowIndex].Top, PanelConfigs[rowIndex].Width, PanelConfigs[rowIndex].Height, false);
                        break;
                    case PanelConfigDataColumn.Top:
                        PInvoke.MoveWindow(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].Left, PanelConfigs[rowIndex].Top + changeAmount, PanelConfigs[rowIndex].Width, PanelConfigs[rowIndex].Height, false);
                        break;
                    case PanelConfigDataColumn.Width:
                        PInvoke.MoveWindow(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].Left, PanelConfigs[rowIndex].Top, PanelConfigs[rowIndex].Width + changeAmount, PanelConfigs[rowIndex].Height, false);
                        MSFSBugPanelShiftWorkaround(PanelConfigs[rowIndex].PanelHandle, orignalLeft, PanelConfigs[rowIndex].Top, PanelConfigs[rowIndex].Width + changeAmount, PanelConfigs[rowIndex].Height);
                        break;
                    case PanelConfigDataColumn.Height:
                        PInvoke.MoveWindow(PanelConfigs[rowIndex].PanelHandle, PanelConfigs[rowIndex].Left, PanelConfigs[rowIndex].Top, PanelConfigs[rowIndex].Width, PanelConfigs[rowIndex].Height + changeAmount, false);
                        MSFSBugPanelShiftWorkaround(PanelConfigs[rowIndex].PanelHandle, orignalLeft, PanelConfigs[rowIndex].Top, PanelConfigs[rowIndex].Width, PanelConfigs[rowIndex].Height + changeAmount);
                        break;
                    default:
                        return;
                }
            }
        }

        private void MSFSBugPanelShiftWorkaround(IntPtr handle, int originalLeft, int top, int width, int height)
        {
            // Fixed MSFS bug, create workaround where on 2nd or later instance of width adjustment, the panel shift to the left by itself
            // Wait for system to catch up on panel coordinate that were just applied
            System.Threading.Thread.Sleep(200);
            
            Rectangle rectangle;
            PInvoke.GetWindowRect(handle, out rectangle);

            if (rectangle.Left != originalLeft)
                PInvoke.MoveWindow(handle, originalLeft, top, width, height, false);
        }

        private void HandlePopOutCompleted(object sender, EventArgs e)
        {
            // Populate panel data
            BaseController.ActiveUserPlaneProfile.PanelConfigs.ForEach(p => PanelConfigs.Add(p));

            // Setup panel config event hooks
            _winEventHook = PInvoke.SetWinEventHook(EVENT_OBJECT_LOCATIONCHANGE, EVENT_OBJECT_LOCATIONCHANGE, IntPtr.Zero, _winEvent, 0, 0, WINEVENT_OUTOFCONTEXT);

        }

        private Rectangle _lastWindowRectangle;

        private void EventCallback(IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero) return;

            var panelConfig = PanelConfigs.FirstOrDefault(panel => panel.PanelHandle == hWnd);

            if (panelConfig != null)
            {
                var rowIndex = PanelConfigs.IndexOf(panelConfig);

                if (panelConfig != null)
                {
                    switch (iEvent)
                    {
                        case EVENT_OBJECT_LOCATIONCHANGE:
                            Rectangle winRectangle;
                            PInvoke.GetWindowRect(panelConfig.PanelHandle, out winRectangle);

                            if (_lastWindowRectangle == winRectangle)       // ignore duplicate callback messages
                                return;

                            _lastWindowRectangle = winRectangle;
                            Rectangle clientRectangle;
                            PInvoke.GetClientRect(panelConfig.PanelHandle, out clientRectangle);

                            panelConfig.Left = winRectangle.Left;
                            panelConfig.Top = winRectangle.Top;
                            panelConfig.Width = clientRectangle.Width + 16;
                            panelConfig.Height = clientRectangle.Height + 39;

                            HightlightSelectedPanel?.Invoke(this, new EventArgs<int>(rowIndex));

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
