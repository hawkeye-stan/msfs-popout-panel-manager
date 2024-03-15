using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.DomainModel.Setting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace MSFSPopoutPanelManager.WindowsAgent
{
    public class TouchEventManager
    {
        private static IntPtr _hHook = IntPtr.Zero;
        private static readonly PInvoke.WindowsHookExProc CallbackDelegate = HookCallBack;
        private static bool _isTouchDownCompleted = true;
        private static bool _isTouchUpCompleted = true;
        private static bool _isDragged;
        private static int _refocusedTaskIndex;

        private static object _lock = new();

        private const int PANEL_MENUBAR_HEIGHT = 31;
        private const uint TOUCH_FLAG = 0xFF515700;
        private const uint WM_MOUSEMOVE = 0x0200;
        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;
        private const uint WM_RBUTTONDOWN = 0x0204;
        private const uint WM_RBUTTONUP = 0x0205;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        public static UserProfile ActiveProfile { private get; set; }

        public static ApplicationSetting ApplicationSetting { private get; set; }

        public static void Hook()
        {
            Debug.WriteLine("Executing touch event manager mouse hook...");

            var curProcess = Process.GetCurrentProcess();
            var curModule = curProcess.MainModule;

            if (curModule == null) 
                return;

            var hookWindowPtr = PInvoke.GetModuleHandle(curModule.ModuleName);
            _hHook = PInvoke.SetWindowsHookEx(HookType.WH_MOUSE_LL, CallbackDelegate, hookWindowPtr, 0);
        }

        public static void UnHook()
        {
            if (_hHook != IntPtr.Zero)
            {
                Debug.WriteLine("Executing touch event manager mouse unhook...");

                PInvoke.UnhookWindowsHookEx(_hHook);
                _hHook = IntPtr.Zero;
            }
        }

        public static bool IsHooked => _hHook != IntPtr.Zero;

        private static int HookCallBack(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code != 0)
                return PInvoke.CallNextHookEx(_hHook, code, wParam, lParam);

            var ptrToStructure = Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

            if (ptrToStructure == null)
                return 0;

            var info = (MSLLHOOKSTRUCT) ptrToStructure;
            var extraInfo = (uint)info.dwExtraInfo;
            var isTouched = (extraInfo & TOUCH_FLAG) == TOUCH_FLAG;

            // Mouse Click
            if (!isTouched)
            {
                switch ((uint)wParam)
                {
                    case WM_LBUTTONDOWN:
                    case WM_LBUTTONUP:
                        if (!_isTouchDownCompleted)
                            return 1;
                        break;
                }

                return PInvoke.CallNextHookEx(_hHook, code, wParam, lParam);
            }

            // If touch point is within pop out panel boundaries and have touch enabled
            var panelConfig = ActiveProfile.PanelConfigs.FirstOrDefault(p => p.TouchEnabled &&
                                                    (info.pt.X > p.Left
                                                    && info.pt.X < p.Left + p.Width
                                                    && info.pt.Y > p.Top + (p.HideTitlebar ? 5 : PANEL_MENUBAR_HEIGHT)
                                                    && info.pt.Y < p.Top + p.Height));

            if (panelConfig == null)
                return PInvoke.CallNextHookEx(_hHook, code, wParam, lParam);

            switch ((uint)wParam)
            {
                case WM_RBUTTONDOWN:
                case WM_RBUTTONUP:
                    return 1;

                case WM_LBUTTONDOWN:
                    if (_isTouchDownCompleted && _isTouchUpCompleted)
                    {
                        _refocusedTaskIndex++;
                        if (panelConfig.PanelType == PanelType.RefocusDisplay)
                            return 1;

                        lock (_lock)
                        {
                            _isTouchDownCompleted = false;
                            _isTouchUpCompleted = false;
                        }

                        Task.Run(() =>
                        {
                            PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, info.pt.X + 1, info.pt.Y + 1, 0, 0); // focus window
                            Thread.Sleep(ApplicationSetting.TouchSetting.TouchDownUpDelay + 10);
                            
                            PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, info.pt.X, info.pt.Y, 0, 0);
                            Thread.Sleep(ApplicationSetting.TouchSetting.TouchDownUpDelay + 25);
                            lock (_lock)
                            {
                                _isTouchDownCompleted = true;
                            }
                        });
                    }
                    return 1;
                case WM_LBUTTONUP:

                    if (panelConfig.PanelType == PanelType.RefocusDisplay)
                    {
                        Task.Run(() =>
                        {
                            lock (_lock)
                            {
                                _isTouchDownCompleted = true;
                                _isDragged = false;
                                _isTouchUpCompleted = true;
                            }

                            // Refocus game window
                            if (ApplicationSetting.RefocusSetting.RefocusGameWindow.IsEnabled && panelConfig.AutoGameRefocus)
                            {
                                var currentRefocusIndex = _refocusedTaskIndex;

                                Thread.Sleep(Convert.ToInt32(ApplicationSetting.RefocusSetting.RefocusGameWindow.Delay * 1000));

                                if (currentRefocusIndex == _refocusedTaskIndex)
                                {
                                    var rect = WindowActionManager.GetWindowRectangle(WindowProcessManager.SimulatorProcess.Handle);
                                    InputEmulationManager.LeftClick(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                                }
                            }
                        });
                    }
                    else
                    {
                        Task.Run(() =>
                        {
                            SpinWait.SpinUntil(() => _isTouchDownCompleted, TimeSpan.FromSeconds(0.25));

                            if (_isDragged)
                            {
                                // Need this for PMS GTN750
                                PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, info.pt.X, info.pt.Y, 0, 0);
                                Thread.Sleep(ApplicationSetting.TouchSetting.TouchDownUpDelay + 25);
                            }

                            PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, info.pt.X, info.pt.Y, 0, 0);

                            lock (_lock)
                            {
                                _isDragged = false;
                                _isTouchDownCompleted = true;
                                _isTouchUpCompleted = true;
                            }

                            // Refocus game window
                            if (ApplicationSetting.RefocusSetting.RefocusGameWindow.IsEnabled && panelConfig.AutoGameRefocus)
                            {
                                var currentRefocusIndex = _refocusedTaskIndex;

                                Thread.Sleep(Convert.ToInt32(ApplicationSetting.RefocusSetting.RefocusGameWindow.Delay * 1000));

                                if (currentRefocusIndex == _refocusedTaskIndex)
                                {
                                    PInvoke.SetForegroundWindow(WindowProcessManager.SimulatorProcess.Handle);

                                    var rect = WindowActionManager.GetWindowRectangle(WindowProcessManager.SimulatorProcess.Handle);
                                    PInvoke.SetCursorPos(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                                }
                            }
                        });
                    }

                    return 1;
                case WM_MOUSEMOVE:
                    if (!_isTouchDownCompleted)
                    {
                        lock (_lock)
                        {
                            _isDragged = true;
                        }

                        return 1;
                    }
                    
                    break;
            }

            return PInvoke.CallNextHookEx(_hHook, code, wParam, lParam);
        }
    }
}
