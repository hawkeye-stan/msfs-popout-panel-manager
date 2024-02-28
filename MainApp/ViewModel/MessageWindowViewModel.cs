using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Colors = System.Windows.Media.Colors;

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
            get => _isVisible;
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

        public MessageWindowViewModel(SharedStorage sharedStorage, PanelSourceOrchestrator panelSourceOrchestrator, PanelPopOutOrchestrator panelPopOutOrchestrator) : base(sharedStorage)
        {
            IsVisible = false;
            panelPopOutOrchestrator.OnPopOutStarted += (_, _) =>
            {
                IsVisible = true;
                WindowWidth = WINDOW_WIDTH_POPOUT_MESSAGE;
                WindowHeight = WINDOW_HEIGHT_POPOUT_MESSAGE;
            };
            panelPopOutOrchestrator.OnPopOutCompleted += (_, _) =>
            {
                Thread.Sleep(1000);
                IsVisible = false;
                WindowWidth = WINDOW_WIDTH_POPOUT_MESSAGE;
                WindowHeight = WINDOW_HEIGHT_POPOUT_MESSAGE;
            };
            panelSourceOrchestrator.OnStatusMessageStarted += (_, _) =>
            {
                IsVisible = true;
                WindowWidth = WINDOW_WIDTH_REGULAR_MESSAGE;
                WindowHeight = WINDOW_HEIGHT_REGULAR_MESSAGE;
            };
            panelSourceOrchestrator.OnStatusMessageEnded += (_, _) =>
            {
                IsVisible = false;
                WindowWidth = WINDOW_WIDTH_REGULAR_MESSAGE;
                WindowHeight = WINDOW_HEIGHT_REGULAR_MESSAGE;
            };

            StatusMessageWriter.OnStatusMessage += (_, e) =>
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

        private List<Run> FormatStatusMessages(List<StatusMessage> messages)
        {
            var runs = new List<Run>
            {
                Capacity = 0
            };

            foreach (var statusMessage in messages)
            {
                var run = new Run
                {
                    Text = statusMessage.Message
                };

                switch (statusMessage.StatusMessageType)
                {
                    case StatusMessageType.Success:
                        run.Foreground = new SolidColorBrush(Colors.LimeGreen);
                        break;
                    case StatusMessageType.Failure:
                        run.Foreground = new SolidColorBrush(Colors.IndianRed);
                        break;
                    case StatusMessageType.Executing:
                        run.Foreground = new SolidColorBrush(Colors.NavajoWhite);
                        break;
                    case StatusMessageType.Info:
                        break;
                }

                runs.Add(run);

                if (statusMessage.NewLine)
                    runs.Add(new Run { Text = Environment.NewLine });
            }

            return runs;
        }
    }
}
