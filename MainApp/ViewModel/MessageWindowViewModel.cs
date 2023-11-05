using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Documents;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class MessageWindowViewModel : BaseViewModel
    {
        private const int WINDOW_WIDTH_POPOUT_MESSAGE = 400;
        private const int WINDOW_HEIGHT_POPOUT_MESSAGE = 225;

        private const int WINDOW_WIDTH_REGULAR_MESSAGE = 300;
        private const int WINDOW_HEIGHT_REGULAR_MESSAGE = 75;

        private bool _isVisible;

        public IntPtr Handle { get; set; }

        public event EventHandler<List<Run>> OnMessageUpdated;

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (!AppSettingData.ApplicationSetting.PopOutSetting.EnablePopOutMessages)
                    return;

                _isVisible = value;
                if (value)
                {
                    CenterDialogToGameWindow();
                }
            }
        }

        public int WindowWidth { get; set; }

        public int WindowHeight { get; set; }

        public MessageWindowViewModel(MainOrchestrator orchestrator) : base(orchestrator)
        {
            IsVisible = false;
            Orchestrator.PanelPopOut.OnPopOutStarted += (sender, e) =>
            {
                IsVisible = true;
                WindowWidth = WINDOW_WIDTH_POPOUT_MESSAGE;
                WindowHeight = WINDOW_HEIGHT_POPOUT_MESSAGE;
            };
            Orchestrator.PanelPopOut.OnPopOutCompleted += (sender, e) =>
            {
                Thread.Sleep(1000);
                IsVisible = false;
                WindowWidth = WINDOW_WIDTH_POPOUT_MESSAGE;
                WindowHeight = WINDOW_HEIGHT_POPOUT_MESSAGE;
            };
            Orchestrator.PanelSource.OnStatusMessageStarted += (sender, e) =>
            {
                IsVisible = true;
                WindowWidth = WINDOW_WIDTH_REGULAR_MESSAGE;
                WindowHeight = WINDOW_HEIGHT_REGULAR_MESSAGE;
            };
            Orchestrator.PanelSource.OnStatusMessageEnded += (sender, e) =>
            {
                IsVisible = false;
                WindowWidth = WINDOW_WIDTH_REGULAR_MESSAGE;
                WindowHeight = WINDOW_HEIGHT_REGULAR_MESSAGE;
            };

            StatusMessageWriter.OnStatusMessage += (sender, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (AppSettingData.ApplicationSetting.PopOutSetting.EnablePopOutMessages)
                    {
                        WindowActionManager.ApplyAlwaysOnTop(Handle, PanelType.StatusMessageWindow, true);
                        OnMessageUpdated?.Invoke(this, FormatStatusMessages(e));

                        CenterDialogToGameWindow();
                    }
                });
            };
        }

        private void CenterDialogToGameWindow()
        {
            if (WindowProcessManager.SimulatorProcess == null)
                return;

            var simulatorRectangle = WindowActionManager.GetWindowRectangle(WindowProcessManager.SimulatorProcess.Handle);
            var left = simulatorRectangle.Left + simulatorRectangle.Width / 2 - WindowWidth / 2;
            var top = simulatorRectangle.Top + simulatorRectangle.Height / 2 - WindowHeight / 2;
            WindowActionManager.MoveWindow(Handle, left, top, WindowWidth, WindowHeight);
            WindowActionManager.ApplyAlwaysOnTop(Handle, PanelType.StatusMessageWindow, true);
        }
    }
}
