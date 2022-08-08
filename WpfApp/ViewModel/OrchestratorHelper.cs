using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.UserDataAgent;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Linq;
using System.Windows;

namespace MSFSPopoutPanelManager.WpfApp.ViewModel
{
    internal class OrchestratorHelper
    {
        private MainOrchestrator _orchestrator;
        private bool _minimizeForPopOut;

        public OrchestratorHelper(MainOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;

            _orchestrator.PanelSource.onOverlayShowed += HandleShowOverlay;
            _orchestrator.PanelSource.OnLastOverlayRemoved += (sender, e) => HandleRemovePanelSourceOverlay(false);
            _orchestrator.PanelSource.OnAllOverlaysRemoved += (sender, e) => HandleRemovePanelSourceOverlay(true);
            _orchestrator.PanelSource.OnSelectionStarted += HandlePanelSelectionStarted;
            _orchestrator.PanelSource.OnSelectionCompleted += HandlePanelSelectionCompleted;

            _orchestrator.PanelPopOut.OnPopOutStarted += HandleOnPopOutStarted;
            _orchestrator.PanelPopOut.OnPopOutCompleted += HandleOnPopOutCompleted;
            _orchestrator.PanelPopOut.OnTouchPanelOpened += HandleOnTouchPanelOpened;

            StatusMessageWriter.OnStatusMessage += HandleOnStatusMessage;
        }

        public Window ApplicationWindow { private get; set; }

        private void HandleOnStatusMessage(object sender, StatusMessageEventArg e)
        {
            if (e.ShowDialog)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var duration = (e.Duration == -1 ? _orchestrator.AppSettingData.AppSetting.OnScreenMessageDuration : e.Duration);

                    if (duration > 0)
                    {
                        var messageDialog = new OnScreenMessageDialog(e.Message, e.StatusMessageType, duration);

                        if (e.DisplayInAppWindow)
                            messageDialog.Owner = ApplicationWindow;

                        messageDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        messageDialog.ShowDialog();
                    }

                    // Bring Pop Out Manager back to foreground on error
                    if (e.StatusMessageType == StatusMessageType.Error)
                    {
                        WindowActionManager.BringWindowToForeground(_orchestrator.ApplicationHandle);
                        ApplicationWindow.Show();
                    }
                });
            }

            // Log to error.log
            if (e.StatusMessageType == StatusMessageType.Error)
            {
                FileLogger.WriteLog($"Application Error: {e.Message}", StatusMessageType.Error);
            }
        }

        private void HandleShowOverlay(object sender, PanelSourceCoordinate e)
        {
            AddPanelCoorOverlay(e);
        }

        private void HandleRemovePanelSourceOverlay(bool removeAll)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                for (int i = Application.Current.Windows.Count - 1; i >= 1; i--)
                {
                    if (Application.Current.Windows[i].GetType() == typeof(PanelCoorOverlay))
                    {
                        Application.Current.Windows[i].Close();
                        if (!removeAll)
                            break;
                    }
                }
            });
        }

        private void AddPanelCoorOverlay(PanelSourceCoordinate panelSourceCoordinate)
        {
            PanelCoorOverlay overlay = new PanelCoorOverlay(panelSourceCoordinate.PanelIndex);
            overlay.IsEditingPanelLocation = _orchestrator.PanelSource.IsEditingPanelSource;
            overlay.WindowStartupLocation = WindowStartupLocation.Manual;
            overlay.MoveWindow(panelSourceCoordinate.X, panelSourceCoordinate.Y);
            overlay.ShowInTaskbar = false;
            overlay.Show();
            overlay.WindowLocationChanged += (sender, e) =>
            {
                if (_orchestrator.ProfileData.ActiveProfile != null)
                {
                    var panelSource = _orchestrator.ProfileData.ActiveProfile.PanelSourceCoordinates.FirstOrDefault(p => p.PanelIndex == panelSourceCoordinate.PanelIndex);
                    if (panelSource != null)
                    {
                        panelSource.X = e.X;
                        panelSource.Y = e.Y;
                        _orchestrator.ProfileData.WriteProfiles();
                    }
                }
            };
        }

        private void HandleOnTouchPanelOpened(object sender, TouchPanelOpenEventArg e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TouchPanelWebViewDialog window = new TouchPanelWebViewDialog(e.PlaneId, e.PanelId, e.Caption, e.Width, e.Height);
                window.Show();
            });
        }

        private void HandleOnPopOutStarted(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Temporary minimize the app for pop out process
                _minimizeForPopOut = ApplicationWindow.WindowState != WindowState.Minimized;
                if (_minimizeForPopOut)
                    WindowActionManager.MinimizeWindow(_orchestrator.ApplicationHandle);
            });
        }

        private void HandleOnPopOutCompleted(object sender, bool isFirstTime)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Restore window state
                if (_minimizeForPopOut && (isFirstTime || !_orchestrator.AppSettingData.AppSetting.MinimizeAfterPopOut))
                {
                    WindowActionManager.BringWindowToForeground(_orchestrator.ApplicationHandle);
                    ApplicationWindow.Show();
                }

                // Important!!! Start WinEventHook from UI thread
                _orchestrator.PanelConfiguration.StartConfiguration();
            });
        }

        private void HandlePanelSelectionStarted(object sender, EventArgs e)
        {
            WindowActionManager.MinimizeWindow(_orchestrator.ApplicationHandle);
        }

        private void HandlePanelSelectionCompleted(object sender, EventArgs e)
        {
            WindowActionManager.BringWindowToForeground(_orchestrator.ApplicationHandle);
            ApplicationWindow.Show();

            if (_orchestrator.ProfileData.ActiveProfile.PanelSourceCoordinates.Count > 0)
                StatusMessageWriter.WriteMessage("Panels selection is completed. Please click 'Start Pop Out' to start popping out these panels.", StatusMessageType.Info, false);
            else
                StatusMessageWriter.WriteMessage("Panels selection is completed. No panel has been selected.", StatusMessageType.Info, false);
        }
    }
}
