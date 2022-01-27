using MahApps.Metro.Controls;
using MSFSPopoutPanelManager.Provider;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;

namespace MSFSPopoutPanelManager.WpfApp
{
    /// <summary>
    /// Interaction logic for OnScreenMessageDialog.xaml
    /// </summary>
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
                var dialogHandle = new WindowInteropHelper(Window.GetWindow(this)).Handle;
                var simulatorProcessHandle = DiagnosticManager.GetSimulatorProcess().Handle;

                Rectangle rectangle;
                PInvoke.GetWindowRect(DiagnosticManager.GetSimulatorProcess().Handle, out rectangle);
                Rectangle clientRectangle;
                PInvoke.GetClientRect(DiagnosticManager.GetSimulatorProcess().Handle, out clientRectangle);

                var x = Convert.ToInt32(rectangle.X + clientRectangle.Width / 2 - this.Width / 2);
                var y = Convert.ToInt32(rectangle.Y + clientRectangle.Height / 2 - this.Height / 2);

                Debug.WriteLine($"Game Location: X:{rectangle.X} Y:{rectangle.Y}");
                Debug.WriteLine($"Game Rectangle: Width:{clientRectangle.Width} Height:{clientRectangle.Height}");
                Debug.WriteLine($"Message Dialog Location: X:{x} Y:{y}");

                PInvoke.MoveWindow(dialogHandle, x, y, Convert.ToInt32(this.Width), Convert.ToInt32(this.Height), false);
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
