using MahApps.Metro.Controls;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;

namespace MSFSPopoutPanelManager.WpfApp
{
    public partial class OnScreenMessageDialog : MetroWindow
    {
        public StatusMessageType StatusMessageType { get; set; }

        public OnScreenMessageDialog(string message, StatusMessageType statusMessageType, int duration)
        {
            InitializeComponent();
            this.DataContext = this;
            this.Topmost = true;
            this.txtMessage.Text = message;
            StatusMessageType = statusMessageType;

            this.Loaded += (sender, e) =>
            {
                var winObj = Window.GetWindow(this);

                if (winObj != null)
                {
                    var window = new WindowInteropHelper(winObj);

                    if (window != null)
                    {
                        var dialogHandle = window.Handle;
                        var simulatorProcess = WindowProcessManager.GetSimulatorProcess();

                        if (simulatorProcess != null)
                        {
                            Rectangle rectangle;
                            PInvoke.GetWindowRect(simulatorProcess.Handle, out rectangle);
                            Rectangle clientRectangle;
                            PInvoke.GetClientRect(simulatorProcess.Handle, out clientRectangle);

                            var x = Convert.ToInt32(rectangle.X + clientRectangle.Width / 2 - this.Width / 2);
                            var y = Convert.ToInt32(rectangle.Y + clientRectangle.Height / 2 - this.Height / 2);

                            WindowActionManager.MoveWindow(dialogHandle, PanelType.WPFWindow, x, y, Convert.ToInt32(this.Width), Convert.ToInt32(this.Height));
                        }
                        else
                        {
                            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        }
                    }
                }
            };

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += (sender, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.Close(); timer.Enabled = false;
                });
            };
            timer.Interval = duration * 1000;
            timer.Enabled = true;
        }
    }
}
