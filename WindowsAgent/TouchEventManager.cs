using MSFSPopoutPanelManager.UserDataAgent;
using System;
using System.Diagnostics;
using System.Drawing;
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
        private static bool _isMouseMoveBlock = false;
        private static object _mouseTouchLock = new object();
        private static int _mouseMoveUnblockCount = 0;
        private static int _mouseMoveCount = 0;
        private static bool _touchUsePreClick = false;
        private static Point _blockPoint;

        private const int PANEL_MENUBAR_HEIGHT = 31;
        private const uint TOUCH_FLAG = 0xFF515700;
        private const uint WM_MOUSEMOVE = 0x0200;
        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;
        private const uint WM_RBUTTONDOWN = 0x0204;
        private const uint WM_RBUTTONUP = 0x0205;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;

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
            if (!ActiveProfile.PanelConfigs.Any(p => p.TouchEnabled
                                                     && info.pt.X > p.Left
                                                    && info.pt.X < p.Left + p.Width
                                                    && info.pt.Y > p.Top + (p.HideTitlebar || p.FullScreen ? 5 : PANEL_MENUBAR_HEIGHT)
                                                    && info.pt.Y < p.Top + p.Height))
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

                        lock (_mouseTouchLock)
                        {
                            Task.Run(() =>
                            {
                                _isMouseMoveBlock = true;

                                Thread.Sleep(25);

                                if (_mouseMoveCount > 1 || _mouseMoveCount > 1)
                                {
                                    PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, _blockPoint.X, _blockPoint.Y, 0, 0);
                                    Thread.Sleep(25);
                                    _touchUsePreClick = true;
                                }

                                PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, _blockPoint.X, _blockPoint.Y, 0, 0);
                                _isMouseMoveBlock = false;
                                _mouseMoveUnblockCount = 0;
                                _mouseMoveCount = 0;
                            });
                        }
                    }
                    break;
                case WM_LBUTTONUP:

                    if (_isTouchDown)
                    {
                        lock (_mouseTouchLock)
                        {
                            Task.Run(() =>
                            {
                                Thread.Sleep(AppSetting.TouchScreenSettings.MouseUpDownDelay + (_touchUsePreClick ? 175 : 125));
                                PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, info.pt.X, info.pt.Y, 0, 0);
                                _isTouchDown = false;
                                _touchUsePreClick = false;
                                RefocusMsfsGameWindow();
                            });
                        }
                        return 1;
                    }
                    return 1;
                case WM_MOUSEMOVE:
                    if (_isMouseMoveBlock)
                    {
                        _mouseMoveUnblockCount++;
                        _blockPoint = new Point(info.pt.X, info.pt.Y);
                        break;
                    }

                    _mouseMoveCount++;
                    break;
            }

            return PInvoke.CallNextHookEx(_hHook, code, wParam, lParam);

        }

        private static void RefocusMsfsGameWindow()
        {
            // ToDo: Adjustable refocus MSFS game, min 500 milliseconds
            Thread.Sleep(1000);

            if (!_isTouchDown && AppSetting.TouchScreenSettings.RefocusGameWindow)
            {
                var rectangle = WindowActionManager.GetWindowRect(_simulatorProcess.Handle);
                var clientRectangle = WindowActionManager.GetClientRect(_simulatorProcess.Handle);

                PInvoke.SetCursorPos(rectangle.X + clientRectangle.Width / 2, rectangle.Y + clientRectangle.Height / 2);
            }
        }
    }
}
