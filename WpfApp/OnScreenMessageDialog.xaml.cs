using MahApps.Metro.Controls;
using MSFSPopoutPanelManager.Model;
using MSFSPopoutPanelManager.Provider;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;

namespace MSFSPopoutPanelManager.WpfApp
{
    public partial class OnScreenMessageDialog : MetroWindow
    {
        public MessageIcon MessageIcon { get; set; }

        public OnScreenMessageDialog(string message) : this(message, MessageIcon.Info, 2) { }

        public OnScreenMessageDialog(string message, MessageIcon messageIcon) : this(message, messageIcon, 2) { }

        public OnScreenMessageDialog(string message, int duration) : this(message, MessageIcon.Info, duration) { }

        public OnScreenMessageDialog(string message, MessageIcon messageIcon, int duration)
        {
            InitializeComponent();
            this.DataContext = this;
            this.Topmost = true;
            this.txtMessage.Text = message;
            MessageIcon = messageIcon;

            this.Loaded += (sender, e) =>
            {
                var winObj = Window.GetWindow(this);

                if (winObj != null)
                {
                    var window = new WindowInteropHelper(winObj);

                    if (window != null)
                    {
                        var dialogHandle = window.Handle;
                        var simulatorProcess = DiagnosticManager.GetSimulatorProcess();

                        if (simulatorProcess != null)
                        {
                            Rectangle rectangle;
                            PInvoke.GetWindowRect(simulatorProcess.Handle, out rectangle);
                            Rectangle clientRectangle;
                            PInvoke.GetClientRect(simulatorProcess.Handle, out clientRectangle);

                            var x = Convert.ToInt32(rectangle.X + clientRectangle.Width / 2 - this.Width / 2);
                            var y = Convert.ToInt32(rectangle.Y + clientRectangle.Height / 2 - this.Height / 2);

                            WindowManager.MoveWindow(dialogHandle, PanelType.WPFWindow, x, y, Convert.ToInt32(this.Width), Convert.ToInt32(this.Height));
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

    public enum MessageIcon
    {
        Info,
        Success,
        Failed
    }
}
