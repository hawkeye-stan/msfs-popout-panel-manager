using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Windows;
using System.Windows.Interop;

namespace MSFSPopoutPanelManager.WpfApp
{
    public partial class PanelCoorOverlay : Window
    {
        private const int TOP_ADJUSTMENT = 23;      // half of window height
        private const int LEFT_ADJUSTMENT = 27;     // half of window width
        private int _xCoor;
        private int _yCoor;

        public bool IsEditingPanelLocation { get; set; }

        public IntPtr WindowHandle { get; set; }

        public event EventHandler<System.Drawing.Point> WindowLocationChanged;

        public PanelCoorOverlay(int panelIndex)
        {
            InitializeComponent();
            this.lblPanelIndex.Content = panelIndex;
            IsEditingPanelLocation = false;
            this.Topmost = true;

            this.LocationChanged += PanelCoorOverlay_LocationChanged;
        }

        public void MoveWindow(int x, int y)
        {
            _xCoor = x - LEFT_ADJUSTMENT;
            _yCoor = y - TOP_ADJUSTMENT;
        }

        private void PanelCoorOverlay_LocationChanged(object sender, EventArgs e)
        {
            if (this.Top is double.NaN || this.Left is double.NaN)
                return;

            // Fixed broken window left/top coordinate for DPI Awareness Per Monitor
            var handle = new WindowInteropHelper(this).Handle;
            var rect = WindowActionManager.GetWindowRect(handle);
            WindowLocationChanged?.Invoke(this, new System.Drawing.Point(rect.X + LEFT_ADJUSTMENT, rect.Y + TOP_ADJUSTMENT));
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsEditingPanelLocation && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Fixed broken window left/top coordinate for DPI Awareness Per Monitor
            var handle = new WindowInteropHelper(this).Handle;
            WindowActionManager.MoveWindow(handle, _xCoor, _yCoor, Convert.ToInt32(this.Width), Convert.ToInt32(this.Height));

            WindowActionManager.ApplyAlwaysOnTop(handle, PanelType.WPFWindow, true);
        }
    }
}
