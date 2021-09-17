using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;

namespace MSFSPopoutPanelManager
{
    public class WindowManager
    {
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        const short SWP_NOZORDER = 0X4;
        const int SWP_SHOWWINDOW = 0x0040;

        private const int MSFS_CONNECTION_RETRY_TIMEOUT = 2000;
        private Timer _timer;
        private UserData _userData;
        private MainWindow _simWindow;
        private AnalysisEngine _analysisEngine;

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

        public void Analyze(string profileName)
        {
            _analysisEngine.Analyze(ref _simWindow, profileName);

            // Load previously saved position if available
            if(_simWindow.ChildWindowsData.Count > 0)
                LoadProfile(profileName);
        }

        public void SaveProfile(string profileName)
        {
            if (_userData == null)
                _userData = new UserData();

            var profile = _userData.Profiles.Find(x => x.Name == profileName);
            if (profile == null)
            {
                profile = new Profile() { Name = profileName };
                _userData.Profiles.Add(profile);
            }

            if (_simWindow.ChildWindowsData.Count > 0)
            {
                foreach (var window in _simWindow.ChildWindowsData)
                {
                    var rect = new Rect();
                    GetWindowRect(window.Handle, ref rect);

                    if (!profile.PopoutNames.TryAdd(window.Title, rect))
                        profile.PopoutNames[window.Title] = rect;
                }

                FileManager.WriteUserData(_userData);
                OnStatusUpdated?.Invoke(this, new EventArgs<string>("Pop out panel positions have been saved."));
            }
        }

        private void LoadProfile(string profileName)
        {
            _userData = FileManager.ReadUserData();
            var profile = _userData != null ? _userData.Profiles.Find(x => x.Name == profileName) : null;
            
            if(profile == null)
            {
                OnStatusUpdated?.Invoke(this, new EventArgs<string>("Position settings for this profile does not exist. Please move pop out panels to desire location and click Save Positions."));
                return;
            }

            if (_simWindow.ChildWindowsData.Count > 0)
            {
                foreach (var window in _simWindow.ChildWindowsData)
                {
                    var coor = profile.PopoutNames.FirstOrDefault(w => w.Key == window.Title);

                    if (!String.IsNullOrEmpty(coor.Key))
                    {
                        var rect = coor.Value;
                        SetWindowPos(window.Handle, 0, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, SWP_NOZORDER | SWP_SHOWWINDOW);
                    }
                }

                OnStatusUpdated?.Invoke(this, new EventArgs<string>("Previously saved panel positions have been set."));
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