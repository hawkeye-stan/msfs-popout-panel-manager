using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;

namespace MSFSPopoutPanelManager
{
    public class WindowManager
    {
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("USER32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

        const int SWP_NOMOVE = 0x0002;  
        const int SWP_NOSIZE = 0x0001;  
        const int SWP_ALWAYS_ON_TOP = SWP_NOMOVE | SWP_NOSIZE;

        const int GWL_STYLE = -16;

        const int WS_SIZEBOX = 0x00040000;
        const int WS_BORDER = 0x00800000;               
        const int WS_DLGFRAME = 0x00400000;             
        const int WS_CAPTION = WS_BORDER | WS_DLGFRAME; 

        private const int MSFS_CONNECTION_RETRY_TIMEOUT = 2000;
        private Timer _timer;
        private UserData _userData;
        private MainWindow _simWindow;
        private AnalysisEngine _analysisEngine;
        private Dictionary<IntPtr, Int64> _originalChildWindowStyles;

        public WindowManager()
        {
            _analysisEngine = new AnalysisEngine();
            _analysisEngine.OnStatusUpdated += (source, e) => OnStatusUpdated?.Invoke(source, e);
            _analysisEngine.OnOcrDebugged += (source, e) => OnOcrDebugged?.Invoke(source, e);
        }
                
        public event EventHandler<EventArgs<string>> OnStatusUpdated;
        public event EventHandler OnSimulatorStarted;
        public event EventHandler<EventArgs<Dictionary<string, string>>> OnOcrDebugged;

        public void CheckSimulatorStarted()
        {
            // Autoconnect to flight simulator
            _timer = new Timer();
            _timer.Interval = MSFS_CONNECTION_RETRY_TIMEOUT;
            _timer.Enabled = true;
            _timer.Elapsed += (source, e) =>
            {
                var simulatorConnected = GetSimulatorWindow();

                if (simulatorConnected)
                {
                    OnSimulatorStarted?.Invoke(this, null);
                    _timer.Enabled = false;
                }
            };
        }

        public bool Analyze(string profileName)
        {
            _originalChildWindowStyles = null;
            _simWindow.ChildWindowsData = new List<ChildWindow>();

            _analysisEngine.Analyze(ref _simWindow, profileName);

            return _simWindow.ChildWindowsData.FindAll(x => x.PopoutType == PopoutType.Custom || x.PopoutType == PopoutType.BuiltIn).Count > 0;
        }

        public void ApplySettings(string profileName, bool showPanelTitleBar, bool alwaysOnTop)
        {
            // Try to load previous profiles
            _userData = FileManager.ReadUserData();
            var profileSettings = _userData != null ? _userData.Profiles.Find(x => x.Name == profileName) : null;

            if (profileSettings == null)
            {
                OnStatusUpdated?.Invoke(this, new EventArgs<string>("Profile settings does not exist. Please move pop out panels to desire location and click Save Settings."));
                return;
            }

            // select all valid windows
            var childWindows = _simWindow.ChildWindowsData.FindAll(x => x.PopoutType == PopoutType.Custom || x.PopoutType == PopoutType.BuiltIn);

            if (childWindows.Count > 0)
            {
                ApplyPositions(profileSettings, childWindows);
                ApplyAlwaysOnTop(alwaysOnTop, childWindows);
                ApplyHidePanelTitleBar(showPanelTitleBar, childWindows);
            }
        }

        public void SaveSettings(string profileName, bool hidePanelTitleBar, bool alwaysOnTop)
        {
            if (_userData == null)
                _userData = new UserData();

            var profile = _userData.Profiles.Find(x => x.Name == profileName);
            if (profile == null)
            {
                profile = new Profile() { Name = profileName, AlwaysOnTop = alwaysOnTop, HidePanelTitleBar = hidePanelTitleBar };
                _userData.Profiles.Add(profile);
            }
            else
            {
                profile.HidePanelTitleBar = hidePanelTitleBar;
                profile.AlwaysOnTop = alwaysOnTop;
            }

            if (_simWindow.ChildWindowsData.Count > 0)
            {
                foreach (var window in _simWindow.ChildWindowsData)
                {
                    if (!window.Title.Contains("Failed Analysis"))
                    {
                        var rect = new Rect();
                        GetWindowRect(window.Handle, ref rect);

                        if (!profile.PopoutNames.TryAdd(window.Title, rect))
                            profile.PopoutNames[window.Title] = rect;
                    }
                }

                FileManager.WriteUserData(_userData);
                OnStatusUpdated?.Invoke(this, new EventArgs<string>("Pop out panel positions have been saved."));
            }
        }

        public void RestorePanelTitleBar()
        {
            if (_simWindow != null)
            {
                var childWindows = _simWindow.ChildWindowsData.FindAll(x => x.PopoutType == PopoutType.Custom || x.PopoutType == PopoutType.BuiltIn);
                ApplyHidePanelTitleBar(false, childWindows);
            }
        }

        private void ApplyPositions(Profile userProfile, List<ChildWindow> childWindows)
        {
            foreach (var childWindow in childWindows)
            {
                var hasCoordinates = userProfile.PopoutNames.ContainsKey(childWindow.Title);

                if (hasCoordinates)
                {
                    var rect = userProfile.PopoutNames[childWindow.Title];
                    MoveWindow(childWindow.Handle, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, true);
                }
            }
        }

        private void ApplyAlwaysOnTop(bool alwaysOnTop, List<ChildWindow> childWindows)
        {
            if (alwaysOnTop)
            {
                foreach (var childWindow in childWindows)
                {   
                    Rect rect = new Rect();
                    GetWindowRect(childWindow.Handle, ref rect);
                    SetWindowPos(childWindow.Handle, new IntPtr(-1), rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, SWP_ALWAYS_ON_TOP);
                }
            }
        }

        private void ApplyHidePanelTitleBar(bool hidePanelTitleBar, List<ChildWindow> childWindows)
        {
            if (hidePanelTitleBar)
            {
                _originalChildWindowStyles = new Dictionary<IntPtr, Int64>();

                foreach (var childWindow in childWindows)
                {
                    // Save the current panel title bar styles so we can restore it later
                    if (!_originalChildWindowStyles.ContainsKey(childWindow.Handle))
                        _originalChildWindowStyles[childWindow.Handle] = GetWindowLong(childWindow.Handle, GWL_STYLE).ToInt64();

                    SetWindowLong(childWindow.Handle, GWL_STYLE, (uint)(_originalChildWindowStyles[childWindow.Handle] & ~(WS_CAPTION | WS_SIZEBOX)));
                }
            }
            else
            {
                if (_originalChildWindowStyles != null)
                {
                    foreach (var childWindow in childWindows)
                    {
                        if (_originalChildWindowStyles.ContainsKey(childWindow.Handle))
                        {
                            SetWindowLong(childWindow.Handle, GWL_STYLE, (uint)_originalChildWindowStyles[childWindow.Handle]);
                            _originalChildWindowStyles.Remove(childWindow.Handle);
                        }
                    }
                }
            }
        }

        private bool GetSimulatorWindow()
        {
            // Get flight simulator process
            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName == "FlightSimulator" && _simWindow == null)
                {
                    _simWindow = new MainWindow()
                    {
                        ProcessId = process.Id,
                        ProcessName = process.ProcessName,
                        Title = "Microsoft Flight Simulator",
                        Handle = process.MainWindowHandle
                    };

                    return true;
                }
            }

            return false;
        }
    }
}