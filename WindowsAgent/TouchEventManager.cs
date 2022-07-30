using MSFSPopoutPanelManager.UserDataAgent;
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
        private static WindowProcess _simulatorProcess;
        private static IntPtr _hHook = IntPtr.Zero;
        private static PInvoke.WindowsHookExProc callbackDelegate = HookCallBack;
        private static bool _isTouchDown;
        private static int _mouseMoveCount;
        private static bool _isMouseMoveBlock = false;
        private static object _mouseTouchLock = new object();
        private static bool _isDragged = false;

        private const int PANEL_MENUBAR_HEIGHT = 31;
        private const uint TOUCH_FLAG = 0xFF515700;
        private const uint WM_MOUSEMOVE = 0x0200;
        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;
        private const uint WM_RBUTTONDOWN = 0x0204;
        private const uint WM_RBUTTONUP = 0x0205;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_MOVE = 0x0001;

        public static Profile ActiveProfile { private get; set; }

        public static AppSetting AppSetting { private get; set; }

        public static void Hook()
        {
            _simulatorProcess = WindowProcessManager.GetSimulatorProcess();

            Process curProcess = Process.GetCurrentProcess();
            ProcessModule curModule = curProcess.MainModule;
            var hookWindowPtr = PInvoke.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
            _hHook = PInvoke.SetWindowsHookEx(HookType.WH_MOUSE_LL, callbackDelegate, hookWindowPtr, 0);
        }

        public static void UnHook()
        {
            PInvoke.UnhookWindowsHookEx(_hHook);
            _hHook = IntPtr.Zero;
        }

        public static bool IsHooked { get { return _hHook != IntPtr.Zero; } }

        private static int HookCallBack(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code != 0)
                return PInvoke.CallNextHookEx(_hHook, code, wParam, lParam);


            var info = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            var extraInfo = (uint)info.dwExtraInfo;
            var isTouched = (extraInfo & TOUCH_FLAG) == TOUCH_FLAG;

            // Mouse Click
            if (!isTouched)
            {
                switch ((uint)wParam)
                {
                    case WM_LBUTTONDOWN:
                    case WM_LBUTTONUP:
                        if (_isTouchDown)
                            return 1;
                        break;
                }

                return PInvoke.CallNextHookEx(_hHook, code, wParam, lParam);
            }

            // Touch
            // If touch point is within pop out panel boundaries and have touch enabled
            var panelConfig = ActiveProfile.PanelConfigs.FirstOrDefault(p => p.TouchEnabled
                                                     && info.pt.X > p.Left
                                                    && info.pt.X < p.Left + p.Width
                                                    && info.pt.Y > p.Top + (p.HideTitlebar || p.FullScreen ? 5 : PANEL_MENUBAR_HEIGHT)
                                                    && info.pt.Y < p.Top + p.Height);

            if (panelConfig == null)
                return PInvoke.CallNextHookEx(_hHook, code, wParam, lParam);

            switch ((uint)wParam)
            {
                case WM_RBUTTONDOWN:
                case WM_RBUTTONUP:
                    return 1;

                case WM_LBUTTONDOWN:
                    if (!_isTouchDown)
                    {
                        _isTouchDown = true;
                        _isMouseMoveBlock = true;

                        lock (_mouseTouchLock)
                        {
                            Task.Run(() =>
                            {
                                if (_mouseMoveCount > 1)
                                {
                                    PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, info.pt.X, info.pt.Y, 0, 0);
                                    _isDragged = true;
                                }

                                Thread.Sleep(AppSetting.TouchScreenSettings.TouchDownUpDelay + 25);
                                PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, info.pt.X, info.pt.Y, 0, 0);
                                Thread.Sleep(AppSetting.TouchScreenSettings.TouchDownUpDelay + 50);
                                _isMouseMoveBlock = false;
                                _isTouchDown = false;
                            });
                        }
                    }
                    return 1;
                case WM_LBUTTONUP:
                    Task.Run(() =>
                    {
                        while (_isTouchDown) { }
                        PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, info.pt.X, info.pt.Y, 0, 0);
                        if (_isDragged)
                        {
                            // Override GTN750 bug - must execute this to fix GTN750 cursor moving offscreen issue when doing touch and drag
                            Thread.Sleep(AppSetting.TouchScreenSettings.TouchDownUpDelay + 50);
                            PInvoke.SetCursorPos(info.pt.X, info.pt.Y);
                            Thread.Sleep(100);
                            InputEmulationManager.LeftClickFast(info.pt.X, info.pt.Y);
                            _isDragged = false;
                        }
                        _mouseMoveCount = 0;
                        RefocusMsfsGameWindow(panelConfig);
                    });
                    return 1;
                case WM_MOUSEMOVE:
                    if (_isMouseMoveBlock)
                        return 1;

                    _mouseMoveCount++;
                    break;
            }

            return PInvoke.CallNextHookEx(_hHook, code, wParam, lParam);

        }

        private static void RefocusMsfsGameWindow(PanelConfig panelConfig)
        {
            Thread.Sleep(AppSetting.TouchScreenSettings.RefocusGameWindowDelay);

            if (!_isTouchDown && AppSetting.TouchScreenSettings.RefocusGameWindow && !panelConfig.DisableGameRefocus)
            {
                var rectangle = WindowActionManager.GetWindowRect(_simulatorProcess.Handle);
                var clientRectangle = WindowActionManager.GetClientRect(_simulatorProcess.Handle);

                PInvoke.SetCursorPos(rectangle.X + clientRectangle.Width / 2, rectangle.Y + clientRectangle.Height / 2);
            }
        }
    }
}
