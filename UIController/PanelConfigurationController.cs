using MSFSPopoutPanelManager.Provider;
using MSFSPopoutPanelManager.Shared;
using System;
using System.Drawing;
using System.Linq;

namespace MSFSPopoutPanelManager.UIController
{
    public class PanelConfigurationController
    {
        private IPanelConfigurationView _view;
        private bool _isDisablePanelChanges;

        private static PInvoke.WinEventProc _winEvent;      // keep this as static to prevent garbage collect or the app will crash
        private IntPtr _winEventHook;

        public PanelConfigurationController(IPanelConfigurationView view, DataStore dataStore)
        {
            _view = view;
            DataStore = dataStore;
            _winEvent = new PInvoke.WinEventProc(EventCallback);
        }

        public DataStore DataStore { get; set; }

        public void Initialize()
        {
            // Populate panel data
            DataStore.PanelConfigs.Clear();
            DataStore.ActiveUserProfile.PanelConfigs.ForEach(p => DataStore.PanelConfigs.Add(p));

            // Setup panel config event hooks
            _winEventHook = PInvoke.SetWinEventHook(PInvokeConstant.EVENT_SYSTEM_MOVESIZEEND, PInvokeConstant.EVENT_OBJECT_LOCATIONCHANGE, WindowManager.GetApplicationProcess().Handle, _winEvent, 0, 0, PInvokeConstant.WINEVENT_OUTOFCONTEXT);

            _view.IsPanelLocked = DataStore.ActiveUserProfile.IsLocked;

            _isDisablePanelChanges = false;
            _view.IsPanelChangeDisabled = false;
        }

        public event EventHandler RefreshDataUI;
        public event EventHandler<EventArgs<int>> HightlightSelectedPanel;

        public void UnhookWinEvent()
        {
            // Unhook all Win API events
            PInvoke.UnhookWinEvent(_winEventHook);
        }

        public void LockPanelChanged(bool isLocked)
        {
            DataStore.ActiveUserProfile.IsLocked = isLocked;
            SaveSettings();

            _view.IsPanelLocked = isLocked;
        }

        public void DisablePanelChanges(bool isDisabled)
        {
            _isDisablePanelChanges = isDisabled;
            _view.IsPanelChangeDisabled = isDisabled;
        }

        public void CellValueChanged(int rowIndex, PanelConfigDataColumn column, object newCellValue)
        {
            if (_isDisablePanelChanges || DataStore.ActiveUserProfile.IsLocked || DataStore.PanelConfigs == null || DataStore.PanelConfigs.Count == 0)
                return;

            int orignalLeft = DataStore.PanelConfigs[rowIndex].Left;

            if (rowIndex != -1)
            {
                switch (column)
                {
                    case PanelConfigDataColumn.PanelName:
                        var name = DataStore.PanelConfigs[rowIndex].PanelName;
                        if (name.IndexOf("(Custom)") == -1)
                            name = name + " (Custom)";

                        PInvoke.SetWindowText(DataStore.PanelConfigs[rowIndex].PanelHandle, name);
                        break;
                    case PanelConfigDataColumn.Left:
                        PInvoke.MoveWindow(DataStore.PanelConfigs[rowIndex].PanelHandle, DataStore.PanelConfigs[rowIndex].Left, DataStore.PanelConfigs[rowIndex].Top, DataStore.PanelConfigs[rowIndex].Width, DataStore.PanelConfigs[rowIndex].Height, true);
                        break;
                    case PanelConfigDataColumn.Top:
                        PInvoke.MoveWindow(DataStore.PanelConfigs[rowIndex].PanelHandle, DataStore.PanelConfigs[rowIndex].Left, DataStore.PanelConfigs[rowIndex].Top, DataStore.PanelConfigs[rowIndex].Width, DataStore.PanelConfigs[rowIndex].Height, true);
                        break;
                    case PanelConfigDataColumn.Width:
                        PInvoke.MoveWindow(DataStore.PanelConfigs[rowIndex].PanelHandle, DataStore.PanelConfigs[rowIndex].Left, DataStore.PanelConfigs[rowIndex].Top, DataStore.PanelConfigs[rowIndex].Width, DataStore.PanelConfigs[rowIndex].Height, true);
                        MSFSBugPanelShiftWorkaround(DataStore.PanelConfigs[rowIndex].PanelHandle, orignalLeft, DataStore.PanelConfigs[rowIndex].Top, DataStore.PanelConfigs[rowIndex].Width, DataStore.PanelConfigs[rowIndex].Height);
                        break;
                    case PanelConfigDataColumn.Height:
                        PInvoke.MoveWindow(DataStore.PanelConfigs[rowIndex].PanelHandle, DataStore.PanelConfigs[rowIndex].Left, DataStore.PanelConfigs[rowIndex].Top, DataStore.PanelConfigs[rowIndex].Width, DataStore.PanelConfigs[rowIndex].Height, true);
                        MSFSBugPanelShiftWorkaround(DataStore.PanelConfigs[rowIndex].PanelHandle, orignalLeft, DataStore.PanelConfigs[rowIndex].Top, DataStore.PanelConfigs[rowIndex].Width, DataStore.PanelConfigs[rowIndex].Height);
                        break;
                    case PanelConfigDataColumn.AlwaysOnTop:
                        DataStore.PanelConfigs[rowIndex].AlwaysOnTop = Convert.ToBoolean(newCellValue);
                        WindowManager.ApplyAlwaysOnTop(DataStore.PanelConfigs[rowIndex].PanelHandle, DataStore.PanelConfigs[rowIndex].AlwaysOnTop, new Rectangle(DataStore.PanelConfigs[rowIndex].Left, DataStore.PanelConfigs[rowIndex].Top, DataStore.PanelConfigs[rowIndex].Width, DataStore.PanelConfigs[rowIndex].Height));
                        break;
                    case PanelConfigDataColumn.HideTitlebar:
                        DataStore.PanelConfigs[rowIndex].HideTitlebar = Convert.ToBoolean(newCellValue);
                        WindowManager.ApplyHidePanelTitleBar(DataStore.PanelConfigs[rowIndex].PanelHandle, DataStore.PanelConfigs[rowIndex].HideTitlebar);
                        break;
                    default:
                        return;
                }

                SaveSettings();
            }
        }

        public void CellValueIncrDecr(int rowIndex, PanelConfigDataColumn column, int changeAmount)
        {
            if (_isDisablePanelChanges || DataStore.ActiveUserProfile.IsLocked || DataStore.PanelConfigs == null || DataStore.PanelConfigs.Count == 0)
                return;

            int orignalLeft = DataStore.PanelConfigs[rowIndex].Left;

            if (rowIndex != -1)
            {
                switch (column)
                {
                    case PanelConfigDataColumn.Left:
                        PInvoke.MoveWindow(DataStore.PanelConfigs[rowIndex].PanelHandle, DataStore.PanelConfigs[rowIndex].Left + changeAmount, DataStore.PanelConfigs[rowIndex].Top, DataStore.PanelConfigs[rowIndex].Width, DataStore.PanelConfigs[rowIndex].Height, false);
                        DataStore.PanelConfigs[rowIndex].Left = DataStore.PanelConfigs[rowIndex].Left + changeAmount;
                        break;
                    case PanelConfigDataColumn.Top:
                        PInvoke.MoveWindow(DataStore.PanelConfigs[rowIndex].PanelHandle, DataStore.PanelConfigs[rowIndex].Left, DataStore.PanelConfigs[rowIndex].Top + changeAmount, DataStore.PanelConfigs[rowIndex].Width, DataStore.PanelConfigs[rowIndex].Height, false);
                        DataStore.PanelConfigs[rowIndex].Top = DataStore.PanelConfigs[rowIndex].Top + changeAmount;
                        break;
                    case PanelConfigDataColumn.Width:
                        PInvoke.MoveWindow(DataStore.PanelConfigs[rowIndex].PanelHandle, DataStore.PanelConfigs[rowIndex].Left, DataStore.PanelConfigs[rowIndex].Top, DataStore.PanelConfigs[rowIndex].Width + changeAmount, DataStore.PanelConfigs[rowIndex].Height, false);
                        MSFSBugPanelShiftWorkaround(DataStore.PanelConfigs[rowIndex].PanelHandle, orignalLeft, DataStore.PanelConfigs[rowIndex].Top, DataStore.PanelConfigs[rowIndex].Width + changeAmount, DataStore.PanelConfigs[rowIndex].Height);
                        DataStore.PanelConfigs[rowIndex].Width = DataStore.PanelConfigs[rowIndex].Width + changeAmount;
                        break;
                    case PanelConfigDataColumn.Height:
                        PInvoke.MoveWindow(DataStore.PanelConfigs[rowIndex].PanelHandle, DataStore.PanelConfigs[rowIndex].Left, DataStore.PanelConfigs[rowIndex].Top, DataStore.PanelConfigs[rowIndex].Width, DataStore.PanelConfigs[rowIndex].Height + changeAmount, false);
                        MSFSBugPanelShiftWorkaround(DataStore.PanelConfigs[rowIndex].PanelHandle, orignalLeft, DataStore.PanelConfigs[rowIndex].Top, DataStore.PanelConfigs[rowIndex].Width, DataStore.PanelConfigs[rowIndex].Height + changeAmount);
                        DataStore.PanelConfigs[rowIndex].Height = DataStore.PanelConfigs[rowIndex].Height + changeAmount;
                        break;
                    default:
                        return;
                }

                SaveSettings();
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

        private Rectangle _lastWindowRectangle;
        private int count = 1;

        private void EventCallback(IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            // check by priority to minimize escaping constraint
            if  (hWnd == IntPtr.Zero
                || idObject != 0
                || hWinEventHook != _winEventHook
                || !(iEvent == PInvokeConstant.EVENT_OBJECT_LOCATIONCHANGE || iEvent == PInvokeConstant.EVENT_SYSTEM_MOVESIZEEND)  
                || _isDisablePanelChanges 
                || DataStore.PanelConfigs == null || DataStore.PanelConfigs.Count == 0)
            {
                return;
            }

            var panelConfig = DataStore.PanelConfigs.FirstOrDefault(panel => panel.PanelHandle == hWnd);

            if (panelConfig != null)
            {
                switch (iEvent)
                {
                    case PInvokeConstant.EVENT_OBJECT_LOCATIONCHANGE:
                        Rectangle winRectangle;
                        PInvoke.GetWindowRect(panelConfig.PanelHandle, out winRectangle);

                        if (_lastWindowRectangle == winRectangle)       // ignore duplicate callback messages
                            return;

                        _lastWindowRectangle = winRectangle;
                        Rectangle clientRectangle;
                        PInvoke.GetClientRect(panelConfig.PanelHandle, out clientRectangle);

                        if (!DataStore.ActiveUserProfile.IsLocked)
                        {
                            panelConfig.Left = winRectangle.Left;
                            panelConfig.Top = winRectangle.Top;
                            panelConfig.Width = clientRectangle.Width + 16;
                            panelConfig.Height = clientRectangle.Height + 39;

                            var rowIndex = DataStore.PanelConfigs.IndexOf(panelConfig);
                            HightlightSelectedPanel?.Invoke(this, new EventArgs<int>(rowIndex));
                        }
                           
                        // Detect if window is maximized, if so, save settings
                        WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
                        wp.length = System.Runtime.InteropServices.Marshal.SizeOf(wp);
                        PInvoke.GetWindowPlacement(hWnd, ref wp);
                        if (wp.showCmd == PInvokeConstant.SW_SHOWMAXIMIZED || wp.showCmd == PInvokeConstant.SW_SHOWMINIMIZED)
                        {
                            if (DataStore.ActiveUserProfile.IsLocked && panelConfig.PanelType == PanelType.CustomPopout)
                                PInvoke.ShowWindow(hWnd, PInvokeConstant.SW_RESTORE);
                            else
                                SaveSettings();
                        }
                            
                        break;
                    case PInvokeConstant.EVENT_SYSTEM_MOVESIZEEND:
                        if(DataStore.ActiveUserProfile.IsLocked && panelConfig.PanelType == PanelType.CustomPopout)
                            PInvoke.MoveWindow(panelConfig.PanelHandle, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height, false);
                        else
                            SaveSettings();
                        break;
                }

                RefreshDataUI?.Invoke(this, null);
            }
        }
        private void SaveSettings()
        {
            var profile = DataStore.ActiveUserProfile;
            profile.PanelConfigs = DataStore.PanelConfigs.ToList();

            var allProfiles = FileManager.ReadUserProfileData();
            var index = allProfiles.FindIndex(x => x.ProfileId == profile.ProfileId);
            allProfiles[index] = profile;
            FileManager.WriteUserProfileData(allProfiles);
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
