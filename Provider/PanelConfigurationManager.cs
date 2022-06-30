using MSFSPopoutPanelManager.Model;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MSFSPopoutPanelManager.Provider
{
    public class PanelConfigurationManager
    {
        private UserProfileManager _userProfileManager;
        private IntPtr _winEventHook;
        private static PInvoke.WinEventProc _winEvent;      // keep this as static to prevent garbage collect or the app will crash
        private Rectangle _lastWindowRectangle;
        private uint _prevWinEvent = PInvokeConstant.EVENT_SYSTEM_CAPTUREEND;
        private int _winEventClickLock = 0;

        public UserProfile UserProfile { get; set; }

        public bool AllowEdit { get; set; }

        public PanelConfigurationManager(UserProfileManager userProfileManager)
        {
            _userProfileManager = userProfileManager;
            _winEvent = new PInvoke.WinEventProc(EventCallback);
            AllowEdit = true;
        }

        public void HookWinEvent()
        {
            // Setup panel config event hooks
            _winEventHook = PInvoke.SetWinEventHook(PInvokeConstant.EVENT_SYSTEM_CAPTURESTART, PInvokeConstant.EVENT_OBJECT_LOCATIONCHANGE, DiagnosticManager.GetApplicationProcess().Handle, _winEvent, 0, 0, PInvokeConstant.WINEVENT_OUTOFCONTEXT);
        }

        public void UnhookWinEvent()
        {
            // Unhook all Win API events
            PInvoke.UnhookWinEvent(_winEventHook);
        }

        public void LockPanelsUpdated()
        {
            UserProfile.IsLocked = !UserProfile.IsLocked;
            _userProfileManager.WriteUserProfiles();
        }

        public void PanelConfigPropertyUpdated(PanelConfigItem panelConfigItem)
        {
            if (panelConfigItem == null || !AllowEdit || UserProfile.IsLocked)
                return;

            var panelConfig = UserProfile.PanelConfigs.ToList().Find(p => p.PanelIndex == panelConfigItem.PanelIndex);

            if (panelConfig != null)
            {
                if (panelConfigItem.PanelConfigProperty == PanelConfigPropertyName.FullScreen)
                {
                    InputEmulationManager.ToggleFullScreenPanel(panelConfig.PanelHandle);
                    panelConfig.HideTitlebar = false;
                    panelConfig.AlwaysOnTop = false;
                }
                else if (panelConfigItem.PanelConfigProperty == PanelConfigPropertyName.PanelName)
                {
                    var name = panelConfig.PanelName;
                    if (name.IndexOf("(Custom)") == -1)
                        name = name + " (Custom)";
                    PInvoke.SetWindowText(panelConfig.PanelHandle, name);
                }
                else if (!panelConfig.FullScreen)
                {
                    switch (panelConfigItem.PanelConfigProperty)
                    {
                        case PanelConfigPropertyName.Left:
                        case PanelConfigPropertyName.Top:
                            WindowManager.MoveWindow(panelConfig.PanelHandle, panelConfig.PanelType, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);
                            break;
                        case PanelConfigPropertyName.Width:
                        case PanelConfigPropertyName.Height:
                            if (panelConfig.HideTitlebar)
                                WindowManager.ApplyHidePanelTitleBar(panelConfig.PanelHandle, false);

                            WindowManager.MoveWindowWithMsfsBugOverrirde(panelConfig.PanelHandle, panelConfig.PanelType, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);

                            if (panelConfig.HideTitlebar)
                                WindowManager.ApplyHidePanelTitleBar(panelConfig.PanelHandle, true);

                            break;
                        case PanelConfigPropertyName.AlwaysOnTop:
                            WindowManager.ApplyAlwaysOnTop(panelConfig.PanelHandle, panelConfig.PanelType, panelConfig.AlwaysOnTop, new Rectangle(panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height));
                            break;
                        case PanelConfigPropertyName.HideTitlebar:
                            WindowManager.ApplyHidePanelTitleBar(panelConfig.PanelHandle, panelConfig.HideTitlebar);
                            break;
                    }
                }

                _userProfileManager.WriteUserProfiles();
            }
        }

        public void PanelConfigIncreaseDecrease(PanelConfigItem panelConfigItem, int changeAmount)
        {
            if (panelConfigItem == null || !AllowEdit || UserProfile.IsLocked || UserProfile.PanelConfigs == null || UserProfile.PanelConfigs.Count == 0)
                return;

            var index = UserProfile.PanelConfigs.ToList().FindIndex(p => p.PanelIndex == panelConfigItem.PanelIndex);

            if (index > -1)
            {
                var panelConfig = UserProfile.PanelConfigs[index];

                // Cannot apply any other settings if panel is full screen mode
                if (panelConfig.FullScreen)
                    return;

                int orignalLeft = panelConfig.Left;

                switch (panelConfigItem.PanelConfigProperty)
                {
                    case PanelConfigPropertyName.Left:
                        panelConfig.Left += changeAmount;
                        WindowManager.MoveWindow(panelConfig.PanelHandle, panelConfig.PanelType, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);
                        break;
                    case PanelConfigPropertyName.Top:
                        panelConfig.Top += changeAmount;
                        WindowManager.MoveWindow(panelConfig.PanelHandle, panelConfig.PanelType, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);
                        break;
                    case PanelConfigPropertyName.Width:
                        panelConfig.Width += changeAmount;

                        if (panelConfig.HideTitlebar)
                            WindowManager.ApplyHidePanelTitleBar(panelConfig.PanelHandle, false);

                        WindowManager.MoveWindowWithMsfsBugOverrirde(panelConfig.PanelHandle, panelConfig.PanelType, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);

                        if (panelConfig.HideTitlebar)
                            WindowManager.ApplyHidePanelTitleBar(panelConfig.PanelHandle, true);

                        break;
                    case PanelConfigPropertyName.Height:
                        panelConfig.Height += changeAmount;

                        if (panelConfig.HideTitlebar)
                            WindowManager.ApplyHidePanelTitleBar(panelConfig.PanelHandle, false);

                        WindowManager.MoveWindowWithMsfsBugOverrirde(panelConfig.PanelHandle, panelConfig.PanelType, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);

                        if (panelConfig.HideTitlebar)
                            WindowManager.ApplyHidePanelTitleBar(panelConfig.PanelHandle, true);

                        break;
                    default:
                        return;
                }

                _userProfileManager.WriteUserProfiles();
            }
        }

        private void EventCallback(IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            switch (iEvent)
            {
                case PInvokeConstant.EVENT_OBJECT_LOCATIONCHANGE:
                case PInvokeConstant.EVENT_SYSTEM_MOVESIZEEND:
                case PInvokeConstant.EVENT_SYSTEM_CAPTURESTART:
                case PInvokeConstant.EVENT_SYSTEM_CAPTUREEND:
                    // check by priority to speed up comparing of escaping constraints
                    if (hWnd == IntPtr.Zero
                        || idObject != 0
                        || hWinEventHook != _winEventHook
                        || !AllowEdit
                        || UserProfile.PanelConfigs == null
                        || UserProfile.PanelConfigs.Count == 0)
                    {
                        return;
                    }

                    HandleEventCallback(hWnd, iEvent);
                    break;
                default:
                    break;
            }
        }

        private void HandleEventCallback(IntPtr hWnd, uint iEvent)
        {
            var panelConfig = UserProfile.PanelConfigs.FirstOrDefault(panel => panel.PanelHandle == hWnd);

            if (panelConfig == null)
                return;

            if (panelConfig.IsLockable && UserProfile.IsLocked)
            {
                switch (iEvent)
                {
                    case PInvokeConstant.EVENT_SYSTEM_MOVESIZEEND:
                        // Move window back to original location
                        PInvoke.MoveWindow(panelConfig.PanelHandle, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height, false);
                        break;
                    case PInvokeConstant.EVENT_OBJECT_LOCATIONCHANGE:
                        // Detect if window is maximized, if so, save settings
                        WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
                        wp.length = System.Runtime.InteropServices.Marshal.SizeOf(wp);
                        PInvoke.GetWindowPlacement(hWnd, ref wp);
                        if (wp.showCmd == PInvokeConstant.SW_SHOWMAXIMIZED || wp.showCmd == PInvokeConstant.SW_SHOWMINIMIZED || wp.showCmd == PInvokeConstant.SW_SHOWNORMAL)
                        {
                            PInvoke.ShowWindow(hWnd, PInvokeConstant.SW_RESTORE);
                        }
                        break;
                    case PInvokeConstant.EVENT_SYSTEM_CAPTUREEND:
                        if (!panelConfig.TouchEnabled || _prevWinEvent == PInvokeConstant.EVENT_OBJECT_LOCATIONCHANGE)
                            break;

                        if (!panelConfig.HasTouchableEvent || _prevWinEvent == PInvokeConstant.EVENT_SYSTEM_CAPTUREEND)
                            break;

                        HandleTouchEvent(panelConfig);
                        break;
                }
            }
            else
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

                        panelConfig.Left = winRectangle.Left;
                        panelConfig.Top = winRectangle.Top;

                        if (panelConfig.HideTitlebar)
                        {
                            panelConfig.Width = clientRectangle.Width;
                            panelConfig.Height = clientRectangle.Height;
                        }
                        else
                        {
                            panelConfig.Width = clientRectangle.Width + 16;
                            panelConfig.Height = clientRectangle.Height + 39;
                        }

                        // Detect if window is maximized, if so, save settings
                        WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
                        wp.length = System.Runtime.InteropServices.Marshal.SizeOf(wp);
                        PInvoke.GetWindowPlacement(hWnd, ref wp);
                        if (wp.showCmd == PInvokeConstant.SW_SHOWMAXIMIZED || wp.showCmd == PInvokeConstant.SW_SHOWMINIMIZED)
                        {
                            _userProfileManager.WriteUserProfiles();
                        }

                        break;
                    case PInvokeConstant.EVENT_SYSTEM_MOVESIZEEND:
                        _userProfileManager.WriteUserProfiles();
                        break;
                    case PInvokeConstant.EVENT_SYSTEM_CAPTUREEND:
                        if (!panelConfig.TouchEnabled || _prevWinEvent == PInvokeConstant.EVENT_OBJECT_LOCATIONCHANGE)
                            break;

                        if (!panelConfig.HasTouchableEvent || _prevWinEvent == PInvokeConstant.EVENT_SYSTEM_CAPTUREEND)
                            break;

                        HandleTouchEvent(panelConfig);
                        break;
                }
            }

            _prevWinEvent = iEvent;
        }

        private void HandleTouchEvent(PanelConfig panelConfig)
        {
            Point point;
            PInvoke.GetCursorPos(out point);

            // Disable left clicking if user is touching the title bar area
            if (point.Y - panelConfig.Top > (panelConfig.HideTitlebar ? 5 : 31))
            {
                var prevWinEventClickLock = ++_winEventClickLock;

                UnhookWinEvent();
                InputEmulationManager.LeftClickFast(point.X, point.Y);
                HookWinEvent();

                if (prevWinEventClickLock == _winEventClickLock)
                {
                    Task.Run(() => RefocusMsfs(prevWinEventClickLock));
                }
            }
        }

        private void RefocusMsfs(int prevWinEventClickLock)
        {
            Thread.Sleep(1000);

            if (prevWinEventClickLock == _winEventClickLock)
            {
                var simulatorProcess = DiagnosticManager.GetSimulatorProcess();

                Rectangle rectangle;
                PInvoke.GetWindowRect(simulatorProcess.Handle, out rectangle);
                PInvoke.SetCursorPos(rectangle.X + 18, rectangle.Y + 80);
            }
        }
    }
}
