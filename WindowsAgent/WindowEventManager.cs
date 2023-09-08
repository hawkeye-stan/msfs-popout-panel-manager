using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.DomainModel.Setting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace MSFSPopoutPanelManager.WindowsAgent
{
    public class WindowEventManager
    {
        private static PInvoke.WinEventProc _winEvent;      // keep this as static to prevent garbage collect or the app will crash
        private static IntPtr _winEventHook;
        private static int? _prevShowWinCmd;

        public static UserProfile ActiveProfile { private get; set; }

        public static ApplicationSetting ApplicationSetting { private get; set; }

        public static void HookWinEvent()
        {
            if (ActiveProfile == null || ActiveProfile.PanelConfigs == null)
                return;

            UnhookWinEvent();
            Debug.WriteLine("Executing HookWinEvent()...");

            bool isRequiredPanelConfiguration = false;
            bool isUsedNontouchPanelRefocusLogic = false;

            if ((!ActiveProfile.IsLocked || (ActiveProfile.IsLocked && ApplicationSetting.PopOutSetting.EnablePanelResetWhenLocked))
                && ActiveProfile.PanelConfigs.Any(p => p.IsPopOutSuccess != null && (bool)p.IsPopOutSuccess))
                isRequiredPanelConfiguration = true;

            if (ActiveProfile.PanelConfigs.Any(p => p.AutoGameRefocus && !p.TouchEnabled && p.IsPopOutSuccess != null && (bool)p.IsPopOutSuccess)
                && !ActiveProfile.PanelConfigs.All(p => p.TouchEnabled)
                && ApplicationSetting.RefocusSetting.RefocusGameWindow.IsEnabled)
                isUsedNontouchPanelRefocusLogic = true;

            uint winEventMin, winEventMax;

            if (isRequiredPanelConfiguration && isUsedNontouchPanelRefocusLogic)
            {
                winEventMin = PInvokeConstant.EVENT_SYSTEM_CAPTURESTART;
                winEventMax = PInvokeConstant.EVENT_OBJECT_STATECHANGE;
            }
            else if (isRequiredPanelConfiguration)
            {
                winEventMin = PInvokeConstant.EVENT_SYSTEM_MOVESIZEEND;
                winEventMax = PInvokeConstant.EVENT_OBJECT_STATECHANGE;
            }
            else if (isUsedNontouchPanelRefocusLogic)
            {
                winEventMin = PInvokeConstant.EVENT_SYSTEM_CAPTURESTART;
                winEventMax = PInvokeConstant.EVENT_SYSTEM_CAPTUREEND;
            }
            else
            {
                return;
            }

            _winEvent = new PInvoke.WinEventProc(EventCallback);
            _winEventHook = PInvoke.SetWinEventHook(winEventMin, winEventMax, IntPtr.Zero, _winEvent, 0, 0, PInvokeConstant.WINEVENT_OUTOFCONTEXT);

            Debug.WriteLine($"WinEventMin: {winEventMin} WinEventMax: {winEventMax}");
        }

        public static void UnhookWinEvent()
        {
            _prevShowWinCmd = null;

            // Unhook all Win API events
            if (_winEventHook != IntPtr.Zero)
            {
                Debug.WriteLine("Executing UnhookWinEvent()...");
                PInvoke.UnhookWinEvent(_winEventHook);
            }
        }

        private static void EventCallback(IntPtr hWinEventHook, uint iEvent, IntPtr hwnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            switch (iEvent)
            {
                case PInvokeConstant.EVENT_SYSTEM_MOVESIZEEND:
                case PInvokeConstant.EVENT_SYSTEM_CAPTURESTART:
                case PInvokeConstant.EVENT_SYSTEM_CAPTUREEND:
                    if (hwnd == IntPtr.Zero || idObject != 0 || hWinEventHook != _winEventHook)   // check by priority to speed up comparing of escaping constraints
                        return;

                    HandleEventCallback(hwnd, iEvent);
                    break;
                case PInvokeConstant.EVENT_OBJECT_STATECHANGE:
                    if (hwnd == IntPtr.Zero || hWinEventHook != _winEventHook)
                        return;

                    HandleEventCallback(hwnd, iEvent);
                    break;
                default:
                    break;
            }
        }

        private static void HandleEventCallback(IntPtr hwnd, uint iEvent)
        {
            var panelConfig = ActiveProfile.PanelConfigs.FirstOrDefault(panel => panel.PanelHandle == hwnd);

            if (panelConfig == null)   // Should not apply any other settings if panel is full screen mode
                return;

            switch (iEvent)
            {
                case PInvokeConstant.EVENT_OBJECT_STATECHANGE:
                    if (!ActiveProfile.IsLocked)
                    {
                        Thread.Sleep(300);
                        UpdatePanelCoordinates(panelConfig);
                    }
                    else
                    {
                        // Pop out is closed
                        var rect = WindowActionManager.GetWindowRectangle(panelConfig.PanelHandle);
                        if (rect.Width == 0 && rect.Height == 0)        
                        {
                            panelConfig.PanelHandle = IntPtr.MaxValue;
                            _prevShowWinCmd = null;
                            return;
                        }

                        WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
                        wp.length = System.Runtime.InteropServices.Marshal.SizeOf(wp);
                        PInvoke.GetWindowPlacement(hwnd, ref wp);

                        if (_prevShowWinCmd == null)
                        {
                            _prevShowWinCmd = wp.showCmd;
                            Thread.Sleep(250);
                            break;
                        }

                        if (wp.showCmd == PInvokeConstant.SW_SHOWMAXIMIZED && _prevShowWinCmd == PInvokeConstant.SW_SHOWNORMAL)
                            PInvoke.ShowWindow(hwnd, PInvokeConstant.SW_RESTORE);
                        else if (wp.showCmd == PInvokeConstant.SW_SHOWMAXIMIZED && _prevShowWinCmd == PInvokeConstant.SW_SHOWMINIMIZED)
                            PInvoke.ShowWindow(hwnd, PInvokeConstant.SW_SHOWMINIMIZED);
                        else if (wp.showCmd == PInvokeConstant.SW_SHOWMINIMIZED && _prevShowWinCmd == PInvokeConstant.SW_SHOWNORMAL)
                            PInvoke.ShowWindow(hwnd, PInvokeConstant.SW_RESTORE);
                        else if (wp.showCmd == PInvokeConstant.SW_SHOWMINIMIZED && _prevShowWinCmd == PInvokeConstant.SW_SHOWMAXIMIZED)
                            PInvoke.ShowWindow(hwnd, PInvokeConstant.SW_SHOWMAXIMIZED);
                        else if (wp.showCmd == PInvokeConstant.SW_SHOWNORMAL && _prevShowWinCmd == PInvokeConstant.SW_SHOWMAXIMIZED)
                            PInvoke.ShowWindow(hwnd, PInvokeConstant.SW_SHOWMAXIMIZED);
                        else if (wp.showCmd == PInvokeConstant.SW_SHOWNORMAL && _prevShowWinCmd == PInvokeConstant.SW_SHOWMINIMIZED)
                            PInvoke.ShowWindow(hwnd, PInvokeConstant.SW_SHOWMINIMIZED);
                        else if (wp.showCmd == PInvokeConstant.SW_SHOWNORMAL && _prevShowWinCmd == PInvokeConstant.SW_SHOWNORMAL)
                            WindowActionManager.MoveWindow(panelConfig.PanelHandle, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);

                        _prevShowWinCmd = null;
                    }
                    break;
                case PInvokeConstant.EVENT_SYSTEM_MOVESIZEEND:
                    if (ActiveProfile.IsLocked)
                        WindowActionManager.MoveWindow(panelConfig.PanelHandle, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);          // Move window back to original location
                    else
                        UpdatePanelCoordinates(panelConfig);
                    break;
                case PInvokeConstant.EVENT_SYSTEM_CAPTURESTART:
                    GameRefocusManager.HandleMouseDownEvent(panelConfig);
                    break;
                case PInvokeConstant.EVENT_SYSTEM_CAPTUREEND:
                    GameRefocusManager.HandleMouseUpEvent(panelConfig);
                    break;
            }
        }

        private static void UpdatePanelCoordinates(PanelConfig panelConfig)
        {
            var rect = WindowActionManager.GetWindowRectangle(panelConfig.PanelHandle);

            if (rect.Width == 0 && rect.Height == 0)        // don't set if width and height = 0
            {
                panelConfig.PanelHandle = IntPtr.MaxValue;
                return;
            }
            
            panelConfig.Left = rect.Left;
            panelConfig.Top = rect.Top;

            if (panelConfig.PanelType == PanelType.HudBarWindow)
                return;

            panelConfig.Width = rect.Width;
            panelConfig.Height = rect.Height;

        }
    }
}
